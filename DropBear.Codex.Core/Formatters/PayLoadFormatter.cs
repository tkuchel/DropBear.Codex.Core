using System.Buffers;
using DropBear.Codex.Core.Models;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class PayloadFormatter<T> : IMessagePackFormatter<Payload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Payload<T> payload, MessagePackSerializerOptions options)
    {
        // Update the array header to account for the new Timestamp property.
        writer.WriteArrayHeader(5); // Now includes data, checksum, signature, public key, and timestamp.

        // Serialize properties in their defined order.
        MessagePackSerializer.Serialize(ref writer, payload.Data, options);
        writer.Write(payload.GetChecksum());
        writer.Write(payload.GetSignature());
        writer.Write(payload.GetExportedPublicKey());
        writer.Write(payload.Timestamp); // Serialize the Timestamp property.
    }

    public Payload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Expecting an array header of 5 elements to match the serialization order.
        var header = reader.ReadArrayHeader();
        if (header != 5)
        {
            throw new InvalidOperationException("Expected array of length 5 for Payload<T> deserialization.");
        }

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
        
        var timestamp = reader.ReadInt64(); // Deserialize the Timestamp property.
        
        // Validate the timestamp.
        if (!ValidateTimestamp(timestamp))
        {
            throw new InvalidOperationException("Invalid timestamp.");
        }
        
        // Use the serialization constructor to create the Payload instance.
        return new Payload<T>(data, checksum, signature, exportedPublicKey, timestamp);
    }
    
    private static bool ValidateTimestamp(long timestamp)
    {
        // Example validation logic:
        // Check if the timestamp is within an acceptable range (e.g., the last 5 minutes)
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return timestamp <= now && timestamp >= now - (5 * 60 * 1000);
    }

}

