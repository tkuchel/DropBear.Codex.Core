using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace DropBear.Codex.Core;

#pragma warning disable MA0048
public class ResultWithPayload<T>
#pragma warning restore MA0048
{
    internal ResultWithPayload(byte[] payload, string hash, bool isSuccess, string error)
    {
        Payload = payload;
        Hash = hash;
        IsSuccess = isSuccess;
        Error = error;
    }

#pragma warning disable CA1819
    public byte[] Payload { get; }
#pragma warning restore CA1819
    public string Hash { get; }
    public bool IsSuccess { get; }
    public string Error { get; private set; }

    public Result<T?> DecompressAndDeserialize()
    {
        if (!IsSuccess)
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
}
