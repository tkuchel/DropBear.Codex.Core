using System.Buffers;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> payload, MessagePackSerializerOptions options)
    {
        // Ensure the number of elements written matches the header.
        writer.WriteArrayHeader(4); // Corrected to include all elements: data, checksum, signature, and public key.

        // Serialize data first, as it's key 0.
        MessagePackSerializer.Serialize(ref writer, payload.Data, options);

        // Follow the order of properties as defined in Payload<T>.
        writer.Write(payload.GetChecksum());
        writer.Write(payload.GetSignature());
        writer.Write(payload.GetExportedPublicKey()); // Assuming a getter method for the public key similar to others.
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Expecting an array header of 4 elements.
        var header = reader.ReadArrayHeader();
        if (header != 4)
            throw new InvalidOperationException("Expected array of length 4 for Payload<T> deserialization.");

        // Deserialize data, which is the first element.
        var data = MessagePackSerializer.Deserialize<T>(ref reader, options);

        // Read checksum, signature, and public key in the order they were written.
        var checksumSequence = reader.ReadBytes();
        var checksum = checksumSequence.HasValue ? checksumSequence.Value.ToArray() : Array.Empty<byte>();

        var signatureSequence = reader.ReadBytes();
        var signature = signatureSequence.HasValue ? signatureSequence.Value.ToArray() : Array.Empty<byte>();

        var exportedPublicKeySequence = reader.ReadBytes();
        var exportedPublicKey = exportedPublicKeySequence.HasValue
            ? exportedPublicKeySequence.Value.ToArray()
            : Array.Empty<byte>();

        // Use the serialization constructor to create the Payload instance.
        return new Payload<T>(data, checksum, signature, exportedPublicKey);
    }
}

