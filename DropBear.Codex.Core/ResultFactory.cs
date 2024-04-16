using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DropBear.Codex.Core;

public static class ResultFactory
{
    // Include methods for Result<T>
    public static Result<T> Success<T>(T value) => new(value, string.Empty, default, Result<T>.ResultState.Success);

    public static Result<T> Failure<T>(string error, Exception? exception = null) =>
        new(default!, error, exception, Result<T>.ResultState.Failure);

    public static Result<T> Failure<T>(Exception exception) =>
        new(default!, exception.Message, exception, Result<T>.ResultState.Failure);

    // Methods for Result<TSuccess, TFailure>
    public static Result<TSuccess, TFailure> Success<TSuccess, TFailure>(TSuccess success) =>
        new(success, default!, Result<TSuccess, TFailure>.ResultState.Success);

    public static Result<TSuccess, TFailure> Failure<TSuccess, TFailure>(TFailure failure) =>
        new(default!, failure, Result<TSuccess, TFailure>.ResultState.Failure);

    // Methods for ResultWithPayload<T>
    public static ResultWithPayload<T> SuccessWithPayload<T>(T data)
    {
        var jsonData = JsonSerializer.Serialize(data);
        var compressedData = Compress(Encoding.UTF8.GetBytes(jsonData));
        var hash = ComputeHash(compressedData);

        return new ResultWithPayload<T>(compressedData, hash, true, string.Empty);
    }

    public static ResultWithPayload<T> FailureWithPayload<T>(string error) => new([], string.Empty, false, error);

    public static async Task<ResultWithPayload<T>> SuccessWithPayloadAsync<T>(T data)
    {
        var jsonData = JsonSerializer.Serialize(data);
        var compressedData = await CompressAsync(Encoding.UTF8.GetBytes(jsonData)).ConfigureAwait(false);
        var hash = await ComputeHashAsync(compressedData).ConfigureAwait(false);

        return new ResultWithPayload<T>(compressedData, hash, true, string.Empty);
    }

    private static async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        var zip = new GZipStream(output, CompressionMode.Compress);
        await using (zip.ConfigureAwait(false))
        {
            await zip.WriteAsync(data).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    private static async Task<string> ComputeHashAsync(byte[] data)
    {
        var hashData = await Task.Run(() => SHA256.HashData(data)).ConfigureAwait(false);
        return Convert.ToBase64String(hashData);
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

    private static string ComputeHash(byte[] data) => Convert.ToBase64String(SHA256.HashData(data));
}
