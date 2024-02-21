using System.Buffers;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> value, MessagePackSerializerOptions options)
    {
        // Ensure to serialize all necessary properties of Payload<T>, including the checksum and data.
        // This is a simplified example. You'll need to adjust it based on the actual structure of Payload<T>.
        writer.WriteArrayHeader(2);
        writer.Write(value.Checksum);
        MessagePackSerializer.Serialize(ref writer, value.Data, options);
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var checksumSequence = reader.ReadBytes();
        var checksum = checksumSequence.HasValue ? checksumSequence.Value.ToArray() : Array.Empty<byte>();
        var data = MessagePackSerializer.Deserialize<T>(ref reader, options);

        // Create a new Payload instance which will compute the checksum for 'data'.
        var payload = Payload<T>.Create(data);

        // Now, validate the checksum of the deserialized data against the checksum read from the serialized payload.
        // Note: Since ComputeChecksum is private and checksum calculation occurs inside the Payload constructor,
        // we can't directly compute and compare checksums here. Instead, we ensure the newly computed checksum
        // in Payload matches the original, which indirectly validates the data integrity.
        if (!payload.Checksum.SequenceEqual(checksum))
            throw new InvalidOperationException("Checksum validation failed. The data may have been tampered with.");

        return payload;
    }
}