using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Core.Utilities;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DropBear.Codex.Core.Extensions;

public static class ResultExtensions
{
    /// <summary>
    ///     Serializes the result to a byte array using MessagePack.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <param name="result">The result to serialize.</param>
    /// <returns>A byte array representing the serialized result.</returns>
    public static byte[] SerializeWithMessagePack<T>(this Result<T> result) where T : notnull
    {
        return MessagePackSerializer.Serialize(result,
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
    }

    /// <summary>
    ///     Wraps a <see cref="Result" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an asynchronous
    ///     method.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="Result" />.</returns>
    public static Task<Result> ToTask(this Result result)
    {
        return Task.FromResult(result);
    }

    /// <summary>
    ///     Wraps a <see cref="Result{T}" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an asynchronous
    ///     method.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the <see cref="Result{T}" />.</typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="Result{T}" />.</returns>
    public static Task<Result<T>> ToTask<T>(this Result<T> result) where T : notnull
    {
        return Task.FromResult(result);
    }

    /// <summary>
    ///     Wraps a <see cref="ResultWithPayload{T}" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an
    ///     asynchronous method.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the <see cref="ResultWithPayload{T}" />.</typeparam>
    /// <param name="result">The <see cref="ResultWithPayload{T}" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="ResultWithPayload{T}" />.</returns>
    public static Task<ResultWithPayload<T>> ToTask<T>(this ResultWithPayload<T> result) where T : notnull
    {
        return Task.FromResult(result);
    }


    /// <summary>
    ///     Serializes and compresses a <see cref="Result{T}" /> using MessagePack with LZ4 block array compression.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <param name="result">The result to serialize and compress.</param>
    /// <returns>A byte array containing the serialized and compressed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result is null.</exception>
    /// <exception cref="MessagePackSerializationException">Thrown if serialization fails.</exception>
    public static byte[] SerializeWithMessagePackAndCompress<T>(this Result<T> result) where T : notnull
    {
        if (result == null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        return MessagePackSerializer.Serialize(result,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithSecurity(MessagePackSecurity.UntrustedData));
    }

    /// <summary>
    ///     Deserializes and decompresses a byte array to a <see cref="Result{T}" /> using MessagePack with LZ4 block array
    ///     compression.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the deserialized result.</typeparam>
    /// <param name="compressedData">The byte array containing the serialized and compressed result.</param>
    /// <returns>A <see cref="Result{T}" /> deserialized from the compressed data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if compressedData is null or empty.</exception>
    /// <exception cref="MessagePackSerializationException">Thrown if deserialization fails.</exception>
    public static Result<T> DeserializeAndDecompressWithMessagePack<T>(byte[] compressedData) where T : notnull
    {
        if (compressedData == null || compressedData.Length == 0)
            throw new ArgumentNullException(nameof(compressedData), "Compressed data cannot be null or empty.");

        return MessagePackSerializer.Deserialize<Result<T>>(compressedData,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithSecurity(MessagePackSecurity.UntrustedData));
    }

    /// <summary>
    ///     Serializes a ResultWithPayload instance to a byte array using MessagePack, including a checksum for data integrity
    ///     verification.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <param name="result">The ResultWithPayload instance to serialize.</param>
    /// <returns>A byte array representing the serialized result with checksum.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result is null.</exception>
    public static byte[] SerializeWithChecksum<T>(this ResultWithPayload<T> result) where T : notnull
    {
        if (result == null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        // Assuming ResultWithPayload<T> can be directly serialized with MessagePack.
        // Adjust serialization options as necessary.
        return MessagePackSerializer.Serialize(result,
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
    }

    /// <summary>
    ///     Deserializes a byte array to a ResultWithPayload instance using MessagePack, verifying the data integrity with a
    ///     checksum.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the deserialized result.</typeparam>
    /// <param name="serializedData">The byte array containing the serialized result with checksum.</param>
    /// <returns>A ResultWithPayload instance deserialized from the byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if serializedData is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if data integrity check fails.</exception>
    public static ResultWithPayload<T> DeserializeWithChecksum<T>(byte[] serializedData) where T : notnull
    {
        if (serializedData == null || serializedData.Length == 0)
            throw new ArgumentNullException(nameof(serializedData), "Serialized data cannot be null or empty.");

        var result = MessagePackSerializer.Deserialize<ResultWithPayload<T>>(serializedData,
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));

        if (result.Payload != null && !result.Payload.ValidateChecksum())
            throw new InvalidOperationException("Data integrity check failed. The data may have been tampered with.");

        return result;
    }


    /// <summary>
    ///     Converts a <see cref="Result" /> to an appropriate <see cref="IActionResult" /> based on its success or failure
    ///     state.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> instance to convert.</param>
    /// <returns>An <see cref="IActionResult" /> that represents the HTTP response for the given result.</returns>
    public static IActionResult ToActionResult(this Result result)
    {
        var statusCode = ExitCodeHttpStatusMapper.GetStatusCode(result.ExitCode);

        if (result.IsSuccess)
            return new OkResult(); // Consider using OkResult for a more consistent approach.
        return new ObjectResult(result.ErrorMessage) { StatusCode = (int)statusCode };
    }

    /// <summary>
    ///     Converts a <see cref="Result{T}" /> to an appropriate <see cref="IActionResult" /> based on its success or failure
    ///     state.
    ///     This overload handles results that include a value on success.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result on success.</typeparam>
    /// <param name="result">The <see cref="Result{T}" /> instance to convert.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that represents the HTTP response for the given result, including the value if
    ///     the operation was successful.
    /// </returns>
    public static IActionResult ToActionResult<T>(this Result<T> result) where T : notnull
    {
        var statusCode = ExitCodeHttpStatusMapper.GetStatusCode(result.ExitCode);

        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : new ObjectResult(result.ErrorMessage) { StatusCode = (int)statusCode };
    }

    /// <summary>
    ///     Converts a <see cref="ResultWithPayload{T}" /> to an appropriate <see cref="IActionResult" /> based on its success
    ///     or failure state.
    ///     This overload handles results that include a payload on success.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result on success.</typeparam>
    /// <param name="result">The <see cref="ResultWithPayload{T}" /> instance to convert.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that represents the HTTP response for the given result,
    ///     including the payload if the operation was successful.
    /// </returns>
    public static IActionResult ToActionResult<T>(this ResultWithPayload<T> result) where T : notnull
    {
        return result.IsSuccess switch
        {
            true when result.Payload != null => new OkObjectResult(result.Payload),
            true => new NoContentResult(),
            _ => new ObjectResult(result.ErrorMessage) { StatusCode = StatusCodes.Status400BadRequest }
        };
    }
}