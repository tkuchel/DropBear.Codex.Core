using System.Buffers;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(7); // Reflects the total number of properties being serialized.

        MessagePackSerializer.Serialize(ref writer, value.Data, options);
        writer.Write(value._checksum);
        writer.Write(value._signature);
        writer.Write(value._publicKey);
        writer.Write(value.Timestamp);
        writer.Write(value.DataType);
        writer.Write(value.PayloadId.ToString());
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.ReadArrayHeader() is not 7)
            throw new InvalidOperationException("Expected array of length 7 for Payload<T> deserialization.");

        T data = MessagePackSerializer.Deserialize<T>(ref reader, options);
        var checksum = reader.ReadBytes()?.ToArray() ?? Array.Empty<byte>();
        var signature = reader.ReadBytes()?.ToArray() ?? Array.Empty<byte>();
        var publicKey = reader.ReadBytes()?.ToArray() ?? Array.Empty<byte>();
        var timestamp = reader.ReadInt64();
        var dataType = reader.ReadString();
        Guid payloadId = new Guid(reader.ReadString() ?? throw new InvalidOperationException("Payload Id is null."));
        return new Payload<T>(data, checksum, signature, publicKey, timestamp, dataType, payloadId);
    }
}