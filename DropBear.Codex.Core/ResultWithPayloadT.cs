using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace DropBear.Codex.Core;

#pragma warning disable MA0048
public class ResultWithPayload<T> : IEquatable<ResultWithPayload<T>>
#pragma warning restore MA0048
{

    internal ResultWithPayload(byte[] payload, string hash, ResultState state, string error)
    {
        Payload = payload;
        Hash = hash;
        State = state;
        Error = error;
    }

#pragma warning disable CA1819
    public byte[] Payload { get; }
#pragma warning restore CA1819
    public string Hash { get; }
    public ResultState State { get; }
    public string Error { get; private set; }

    public bool Equals(ResultWithPayload<T>? other)
    {
        if (other is null) return false;
        return State == other.State && Hash == other.Hash && Payload.SequenceEqual(other.Payload);
    }

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

    public override bool Equals(object? obj) => Equals(obj as ResultWithPayload<T>);
    public override int GetHashCode() => HashCode.Combine(State, Hash, Payload);
}
