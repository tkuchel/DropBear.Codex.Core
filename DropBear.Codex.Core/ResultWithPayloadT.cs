#region

#region

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

#endregion

namespace DropBear.Codex.Core;

#endregion

public class ResultWithPayload<T> : IEquatable<ResultWithPayload<T>>
{
    internal ResultWithPayload(byte[]? payload, string? hash, ResultState state, string? errorMessage)
    {
        Payload = payload ?? Array.Empty<byte>();
        Hash = hash ?? string.Empty;
        State = state;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public byte[] Payload { get; }
    public string Hash { get; }
    public ResultState State { get; }
    public string ErrorMessage { get; private set; }

    public bool IsValid => State == ResultState.Success && ValidateHash(Payload, Hash);

    public bool Equals(ResultWithPayload<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return State == other.State && Hash == other.Hash && Payload.SequenceEqual(other.Payload);
    }

    public static ResultWithPayload<T> SuccessWithPayload(T data)
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
            return new ResultWithPayload<T>(Array.Empty<byte>(), string.Empty, ResultState.Failure,
                "Serialization failed.");
        }
        catch (Exception ex)
        {
            return new ResultWithPayload<T>(Array.Empty<byte>(), string.Empty, ResultState.Failure, ex.Message);
        }
    }

    public static ResultWithPayload<T> FailureWithPayload(string error)
    {
        return new ResultWithPayload<T>(Array.Empty<byte>(), string.Empty, ResultState.Failure, error);
    }

    public static async Task<ResultWithPayload<T>> SuccessWithPayloadAsync(T data)
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
            return new ResultWithPayload<T>(Array.Empty<byte>(), string.Empty, ResultState.PartialSuccess,
                "Serialization partially failed.");
        }
        catch (Exception ex)
        {
            return new ResultWithPayload<T>(Array.Empty<byte>(), string.Empty, ResultState.Failure, ex.Message);
        }
    }

    public void UpdateErrorMessage(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public Result<T?> DecompressAndDeserialize()
    {
        if (State is not ResultState.Success)
        {
            return Result<T?>.Failure("Operation failed, cannot decompress.");
        }

        try
        {
            using var input = new MemoryStream(Payload);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var decompressedJson = reader.ReadToEnd();

            if (!ValidateHash(Payload, Hash))
            {
                throw new InvalidOperationException("Data corruption detected during decompression.");
            }

            var deserializedData = JsonSerializer.Deserialize<T>(decompressedJson);
            return deserializedData is not null
                ? Result<T?>.Success(deserializedData)
                : Result<T?>.Failure("Deserialization returned null.");
        }
        catch (Exception ex)
        {
            return Result<T?>.Failure(ex.Message);
        }
    }

    private static bool ValidateHash(byte[] data, string expectedHash)
    {
        var actualHash = ComputeHash(data);
        return actualHash == expectedHash;
    }

    private static string ComputeHash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToBase64String(hash);
    }

    private static async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zip = new GZipStream(output, CompressionMode.Compress))
        {
            await zip.WriteAsync(data).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zip = new GZipStream(output, CompressionMode.Compress))
        {
            zip.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ResultWithPayload<T>);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(State, Hash, Payload);
    }
}
