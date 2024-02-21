using System.Security.Cryptography;
using MessagePack;

namespace DropBear.Codex.Core.Models;

public class Payload<T> where T : notnull
{
    // Private constructor to enforce the use of factory method
    private Payload(T data)
    {
        Data = data;
        Checksum = ComputeChecksum(data);
    }

    public T Data { get; }
    public byte[] Checksum { get; }
    public DateTimeOffset? Timestamp { get; private init; } = DateTimeOffset.UtcNow;
    public string ContentType { get; private init; } = "application/msgpack";

    /// <summary>
    ///     Computes and sets the checksum for the current data using SHA-256.
    /// </summary>
    private byte[] ComputeChecksum(T data)
    {
        var serializedData = MessagePackSerializer.Serialize(data);
        return SHA256.HashData(serializedData);
    }

    /// <summary>
    ///     Creates a payload from the specified data, automatically computing its checksum.
    /// </summary>
    /// <param name="data">The data to encapsulate.</param>
    /// <returns>A new payload instance with computed checksum.</returns>
    public static Payload<T> Create(T data)
    {
        return new Payload<T>(data);
    }

    /// <summary>
    ///     Validates the integrity of the data by comparing the computed checksum with the one stored in the payload.
    /// </summary>
    /// <returns>True if the checksums match, indicating the data has not been tampered with; otherwise, false.</returns>
    public bool ValidateChecksum()
    {
        var computedChecksum = ComputeChecksum(Data);
        return Checksum.SequenceEqual(computedChecksum);
    }
}