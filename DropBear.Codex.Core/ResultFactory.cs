using System.Diagnostics.Contracts;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DropBear.Codex.Core;

/// <summary>
///     Factory class for creating instances of <see cref="Result{T}" /> and <see cref="Result{TSuccess, TFailure}" />.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="value">The value of the successful result.</param>
    /// <returns>A successful result with the specified value.</returns>
    [Pure]
    public static Result<T> Success<T>(T value) => new(value, string.Empty, default, ResultState.Success);

    /// <summary>
    ///     Creates a failure result with the specified error message and optional exception.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if available.</param>
    /// <returns>A failure result with the specified error message and optional exception.</returns>
    [Pure]
    public static Result<T> Failure<T>(string error, Exception? exception = null) =>
        new(default!, error, exception, ResultState.Failure);

    /// <summary>
    ///     Creates a failure result with the specified exception.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failure result with the specified exception.</returns>
    public static Result<T> Failure<T>(Exception exception) =>
        new(default!, exception.Message, exception, ResultState.Failure);

    /// <summary>
    ///     Creates a successful result with the specified success value and no failure value.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the failure value.</typeparam>
    /// <param name="success">The success value.</param>
    /// <returns>A successful result with the specified success value and no failure value.</returns>
    public static Result<TSuccess, TFailure> Success<TSuccess, TFailure>(TSuccess success) =>
        new(success, default!, ResultState.Success);

    /// <summary>
    ///     Creates a failure result with the specified failure value and no success value.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the failure value.</typeparam>
    /// <param name="failure">The failure value.</param>
    /// <returns>A failure result with the specified failure value and no success value.</returns>
    public static Result<TSuccess, TFailure> Failure<TSuccess, TFailure>(TFailure failure) =>
        new(default!, failure, ResultState.Failure);

    /// <summary>
    ///     Creates a successful result with the specified data payload.
    /// </summary>
    /// <typeparam name="T">The type of the data payload.</typeparam>
    /// <param name="data">The data payload to include in the result.</param>
    /// <returns>A successful result with the specified data payload.</returns>
    [Pure]
    public static ResultWithPayload<T> SuccessWithPayload<T>(T data)
    {
        var jsonData = JsonSerializer.Serialize(data);
        var compressedData = Compress(Encoding.UTF8.GetBytes(jsonData));
        var hash = ComputeHash(compressedData);

        return new ResultWithPayload<T>(compressedData, hash, ResultState.Success, string.Empty);
    }

    /// <summary>
    ///     Creates a failure result with the specified error message and no data payload.
    /// </summary>
    /// <typeparam name="T">The type of the data payload.</typeparam>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A failure result with the specified error message and no data payload.</returns>
    public static ResultWithPayload<T> FailureWithPayload<T>(string error) =>
        new(Array.Empty<byte>(), string.Empty, ResultState.Failure, error);

    /// <summary>
    ///     Asynchronously creates a successful result with the specified data payload.
    /// </summary>
    /// <typeparam name="T">The type of the data payload.</typeparam>
    /// <param name="data">The data payload to include in the result.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a successful result with the
    ///     specified data payload.
    /// </returns>
    public static async Task<ResultWithPayload<T>> SuccessWithPayloadAsync<T>(T data)
    {
        var jsonData = JsonSerializer.Serialize(data);
        var compressedData = await CompressAsync(Encoding.UTF8.GetBytes(jsonData)).ConfigureAwait(false);
        var hash = await ComputeHashAsync(compressedData).ConfigureAwait(false);

        return new ResultWithPayload<T>(compressedData, hash, ResultState.Success, string.Empty);
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
