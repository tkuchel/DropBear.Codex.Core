using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation that includes a payload, a hash value, and error information.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
#pragma warning disable MA0048
public class ResultWithPayload<T> : IEquatable<ResultWithPayload<T>>
#pragma warning restore MA0048
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResultWithPayload{T}" /> class.
    /// </summary>
    /// <param name="payload">The payload data.</param>
    /// <param name="hash">The hash value of the payload.</param>
    /// <param name="state">The state of the result (Success or Failure).</param>
    /// <param name="error">The error message if the operation failed.</param>
    internal ResultWithPayload(byte[] payload, string hash, ResultState state, string error)
    {
        Payload = payload;
        Hash = hash;
        State = state;
        Error = error;
    }

    /// <summary>
    ///     Gets the payload data.
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
    public byte[] Payload { get; }
#pragma warning restore CA1819

    /// <summary>
    ///     Gets the hash value of the payload.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    ///     Gets the state of the result (Success or Failure).
    /// </summary>
    public ResultState State { get; }

    /// <summary>
    ///     Gets or sets the error message if the operation failed.
    /// </summary>
    public string Error { get; private set; }

    /// <inheritdoc />
    public bool Equals(ResultWithPayload<T>? other)
    {
        if (other is null) return false;
        return State == other.State && Hash == other.Hash && Payload.SequenceEqual(other.Payload);
    }

    /// <summary>
    ///     Decompresses and deserializes the payload data.
    /// </summary>
    /// <returns>A result indicating the success or failure of the operation.</returns>
    public Result<T?> DecompressAndDeserialize()
    {
        if (State is not ResultState.Success)
            return ResultFactory.Failure<T?>("Operation failed, cannot decompress.");

        try
        {
            using var input = new MemoryStream(Payload);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var decompressedJson = reader.ReadToEnd();
            ValidateData(Payload, Hash);
            var deserializedData = JsonSerializer.Deserialize<T>(decompressedJson) ?? default(T);
            return ResultFactory.Success(deserializedData);
        }
        catch (Exception ex)
        {
            return ResultFactory.Failure<T?>(ex.Message);
        }
    }

    private static void ValidateData(byte[] data, string expectedHash)
    {
        var actualHash = ComputeHash(data);
        if (actualHash != expectedHash)
            throw new InvalidOperationException("Data corruption detected during decompression.");
    }

    private static string ComputeHash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToBase64String(hash);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ResultWithPayload<T>);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(State, Hash, Payload);
}
