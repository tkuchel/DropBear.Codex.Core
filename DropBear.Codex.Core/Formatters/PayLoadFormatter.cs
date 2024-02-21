using System.Buffers;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.Checksum);
        MessagePackSerializer.Serialize(ref writer, value.Data, options);
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var expectedCount = 2;
        var actualCount = reader.ReadArrayHeader();
        if (actualCount != expectedCount)
            throw new InvalidOperationException($"Expected 2 elements but got {actualCount}");

        var checksumSequence = reader.ReadBytes();
        var checksum = checksumSequence.HasValue ? checksumSequence.Value.ToArray() : Array.Empty<byte>();

        var data = MessagePackSerializer.Deserialize<T>(ref reader, options);

        var payload = new Payload<T>(data, checksum);
        if (!payload.ValidateChecksum()) throw new InvalidOperationException("Checksum validation failed");

        return payload;
    }
}