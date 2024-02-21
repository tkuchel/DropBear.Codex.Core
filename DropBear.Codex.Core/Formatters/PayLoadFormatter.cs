using System.Buffers;
using DropBear.Codex.Core.Exceptions;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>>
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> payload, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(3);

        writer.Write(payload.GetChecksum());
        writer.Write(payload.GetSignature());

        MessagePackSerializer.Serialize(payload.Data, options);
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var header = reader.ReadArrayHeader();

        if (header != 4)
            throw new PayloadFormatterDeserializeException();
        var checksumSequence = reader.ReadBytes();
        var checksum = checksumSequence.HasValue ? checksumSequence.Value.ToArray() : Array.Empty<byte>();

        var signatureSequence = reader.ReadBytes();
        var signature = signatureSequence.HasValue ? signatureSequence.Value.ToArray() : Array.Empty<byte>();

        var exportedPublicKeySequence = reader.ReadBytes();
        var exportedPublicKey = exportedPublicKeySequence.HasValue
            ? exportedPublicKeySequence.Value.ToArray()
            : Array.Empty<byte>();

        var data = MessagePackSerializer.Deserialize<T>(ref reader, options);

        return new Payload<T>(data, checksum, signature, exportedPublicKey);
    }
}