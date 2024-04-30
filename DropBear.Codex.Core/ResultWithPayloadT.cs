using System.Diagnostics.Contracts;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
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
    ///     Initializes a new instance of the ResultWithPayload&lt;T&gt; class.
    /// </summary>
    /// <param name="payload">The payload data.</param>
    /// <param name="hash">The hash value of the payload.</param>
    /// <param name="state">The state of the result (Success or Failure).</param>
    /// <param name="errorMessage">The errorMessage message if the operation failed.</param>
    internal ResultWithPayload(byte[] payload, string hash, ResultState state, string errorMessage)
    {
        Payload = payload;
        Hash = hash;
        State = state;
        ErrorMessage = errorMessage;
    }

#pragma warning disable CA1819
    public byte[] Payload { get; }
#pragma warning restore CA1819
    public string Hash { get; }
    public ResultState State { get; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string ErrorMessage { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the result is valid.
    ///     The result is valid if the state is Success and the hash value is valid.
    /// </summary>
    [Pure]
    public bool IsValid => State is ResultState.Success && ValidateHash(Payload, Hash);

    public bool Equals(ResultWithPayload<T>? other)
    {
        if (other is null) return false;
        return State == other.State && Hash == other.Hash && Payload.SequenceEqual(other.Payload);
    }

#pragma warning disable CA1000
    public static ResultWithPayload<T> SuccessWithPayload(T data)
#pragma warning restore CA1000
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(data);
            var compressedData = Compress(Encoding.UTF8.GetBytes(jsonData));
            var hash = ComputeHash(compressedData);
            return new ResultWithPayload<T>(compressedData, hash, ResultState.Success, string.Empty);
        }
        catch (JsonException)
        {
            return new ResultWithPayload<T>([], string.Empty, ResultState.Warning,
                "Serialization failed.");
        }
        catch (Exception ex)
        {
            return new ResultWithPayload<T>([], string.Empty, ResultState.Failure, ex.Message);
        }
    }

#pragma warning disable CA1000
    public static ResultWithPayload<T> FailureWithPayload(string error) =>
#pragma warning restore CA1000
        new([], string.Empty, ResultState.Failure, error);

#pragma warning disable CA1000
    public static async Task<ResultWithPayload<T>> SuccessWithPayloadAsync(T data)
#pragma warning restore CA1000
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(data);
            var compressedData = await CompressAsync(Encoding.UTF8.GetBytes(jsonData)).ConfigureAwait(false);
            var hash = ComputeHash(compressedData);
            return new ResultWithPayload<T>(compressedData, hash, ResultState.Success, string.Empty);
        }
        catch (JsonException)
        {
            return new ResultWithPayload<T>([], string.Empty, ResultState.PartialSuccess,
                "Serialization partially failed.");
        }
        catch (Exception ex)
        {
            return new ResultWithPayload<T>([], string.Empty, ResultState.Failure, ex.Message);
        }
    }

    public Result<T?> DecompressAndDeserialize()
    {
        if (State is not ResultState.Success)
            return Result<T?>.Failure("Operation failed, cannot decompress.");

        try
        {
            using var input = new MemoryStream(Payload);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var decompressedJson = reader.ReadToEnd();
            if (!ValidateHash(Payload, Hash))
                throw new InvalidOperationException("Data corruption detected during decompression.");

            var deserializedData = JsonSerializer.Deserialize<T>(decompressedJson) ?? default(T);
            return Result<T?>.Success(deserializedData);
        }
        catch (Exception ex)
        {
            return Result<T?>.Failure(ex.Message);
        }
    }

    [Pure]
    private static bool ValidateHash(byte[] data, string expectedHash)
    {
        var actualHash = ComputeHash(data);
        return actualHash == expectedHash;
    }

    [Pure]
    private static string ComputeHash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToBase64String(hash);
    }

    private static async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        var zip = new GZipStream(output, CompressionMode.Compress);
        await using (zip.ConfigureAwait(false))
        {
            await zip.WriteAsync(data).ConfigureAwait(false);
            return output.ToArray();
        }
    }

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using var zip = new GZipStream(output, CompressionMode.Compress);
        zip.Write(data, 0, data.Length);
        return output.ToArray();
    }

    public override bool Equals(object? obj) => Equals(obj as ResultWithPayload<T>);
    public override int GetHashCode() => HashCode.Combine(State, Hash, Payload);
}
