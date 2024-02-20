using System.Security.Cryptography;
using MessagePack;

public class Payload<T> where T : notnull
{
    // Private constructor to enforce the use of factory method
    private Payload(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data), "Data cannot be null.");
        ComputeAndSetChecksum(); // Ensure checksum is computed upon creation
    }

    public byte[] Data { get; }
    public byte[] Checksum { get; private set; } // Allow setting within class
    public DateTimeOffset? Timestamp { get; private init; } = DateTimeOffset.UtcNow; // Default to current time
    public string ContentType { get; private init; } = "application/msgpack"; // Default to MsgPack

    /// <summary>
    ///     Computes and sets the checksum for the current data using SHA-256.
    /// </summary>
    private void ComputeAndSetChecksum()
    {
        Checksum = SHA256.HashData(Data);
    }

    /// <summary>
    ///     Creates a payload from the specified data, automatically computing its checksum.
    /// </summary>
    /// <param name="data">The data to encapsulate.</param>
    /// <returns>A new payload instance with computed checksum.</returns>
    public static Payload<T> Create(T data)
    {
        var serializedData = MessagePackSerializer.Serialize(data,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithSecurity(MessagePackSecurity.UntrustedData));

        return new Payload<T>(serializedData);
    }

    /// <summary>
    ///     Validates the integrity of the data by comparing the computed checksum with the one stored in the payload.
    /// </summary>
    /// <returns>True if the checksums match, indicating the data has not been tampered with; otherwise, false.</returns>
    public bool ValidateChecksum()
    {
        var computedChecksum = SHA256.HashData(Data);
        return Checksum.SequenceEqual(computedChecksum);
    }
}