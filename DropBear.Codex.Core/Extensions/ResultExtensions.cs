using System.Reflection;
using DropBear.Codex.Core.Resolvers;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Core.Utilities;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DropBear.Codex.Core.Extensions;

public static class ResultExtensions
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithResolver(CustomResolver.Instance)
        .WithSecurity(MessagePackSecurity.UntrustedData);

    /// <summary>
    ///     Ensures that the type <typeparamref name="T" /> is compatible with MessagePack serialization by checking for the
    ///     presence of the <see cref="MessagePackObjectAttribute" /> on the type and the <see cref="KeyAttribute" /> on its
    ///     properties.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack serialization compatibility.</typeparam>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the type <typeparamref name="T" /> does not have the
    ///     <see cref="MessagePackObjectAttribute" />, or if any of its properties meant to be serialized do not have the
    ///     <see cref="KeyAttribute" />.
    /// </exception>
    /// <remarks>
    ///     This method uses reflection to inspect the attributes applied to the type <typeparamref name="T" /> and its
    ///     properties.
    ///     It is designed to be used in development or testing environments to ensure that types are properly annotated for
    ///     MessagePack serialization. Using this method in a production environment is not recommended due to the performance
    ///     overhead associated with reflection.
    /// </remarks>
    public static void EnsureMessagePackCompatibility<T>()
    {
        var type = typeof(T);

        // Check if type is public and not nested
        if (!type.IsPublic || type.IsNested)
            throw new InvalidOperationException($"Type {type.Name} must be public and not nested.");

        var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
        if (messagePackObjectAttribute == null)
            throw new InvalidOperationException($"Type {type.Name} must have a MessagePackObject attribute.");

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var keyAttribute = property.GetCustomAttribute<KeyAttribute>();
            if (keyAttribute == null)
                throw new InvalidOperationException(
                    $"Property {property.Name} in type {type.Name} must have a Key attribute for MessagePack serialization.");
        }
    }

    /// <summary>
    ///     Serializes and compresses a <see cref="Result{T1, T2}" /> using MessagePack with LZ4 block array compression.
    /// </summary>
    /// <typeparam name="T1">The type of the success value encapsulated by the result.</typeparam>
    /// <typeparam name="T2">The type of the failure value encapsulated by the result.</typeparam>
    /// <param name="result">The result to serialize and compress.</param>
    /// <returns>A byte array containing the serialized and compressed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result is null.</exception>
    public static byte[] SerializeWithMessagePackAndCompress<T1, T2>(this Result<T1, T2> result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        var compressionOptions =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Serialize(result, compressionOptions);
    }

    /// <summary>
    ///     Deserializes and decompresses a byte array to a <see cref="Result{T1, T2}" /> using MessagePack with LZ4 block
    ///     array compression.
    /// </summary>
    /// <typeparam name="T1">The type of the success value encapsulated by the deserialized result.</typeparam>
    /// <typeparam name="T2">The type of the failure value encapsulated by the deserialized result.</typeparam>
    /// <param name="compressedData">The byte array containing the serialized and compressed result.</param>
    /// <returns>A <see cref="Result{T1, T2}" /> deserialized from the compressed data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if compressedData is null or empty.</exception>
    public static Result<T1, T2> DeserializeAndDecompressWithMessagePack<T1, T2>(byte[] compressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            throw new ArgumentNullException(nameof(compressedData), "Compressed data cannot be null or empty.");

        var compressionOptions =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Deserialize<Result<T1, T2>>(compressedData, compressionOptions);
    }

    /// <summary>
    ///     Serializes a <see cref="Result{T1, T2}" /> to a byte array using MessagePack without compression.
    /// </summary>
    /// <typeparam name="T1">The type of the success value encapsulated by the result.</typeparam>
    /// <typeparam name="T2">The type of the failure value encapsulated by the result.</typeparam>
    /// <param name="result">The result to serialize.</param>
    /// <returns>A byte array containing the serialized result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result is null.</exception>
    public static byte[] SerializeWithMessagePack<T1, T2>(this Result<T1, T2> result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");
        return MessagePackSerializer.Serialize(result, MessagePackSerializerOptions.Standard);
    }

    /// <summary>
    ///     Deserializes a byte array back into a <see cref="Result{T1, T2}" /> instance using MessagePack without
    ///     decompression.
    /// </summary>
    /// <typeparam name="T1">The type of the success value encapsulated by the result.</typeparam>
    /// <typeparam name="T2">The type of the failure value encapsulated by the result.</typeparam>
    /// <param name="serializedResult">The byte array representing the serialized result.</param>
    /// <returns>The deserialized <see cref="Result{T1, T2}" /> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the serializedResult is null or empty.</exception>
    public static Result<T1, T2> DeserializeWithMessagePack<T1, T2>(byte[] serializedResult)
    {
        if (serializedResult == null || serializedResult.Length == 0)
            throw new ArgumentNullException(nameof(serializedResult), "Serialized result cannot be null or empty.");

        return MessagePackSerializer.Deserialize<Result<T1, T2>>(serializedResult,
            MessagePackSerializerOptions.Standard);
    }

    /// <summary>
    ///     Serializes the result to a byte array using MessagePack.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <param name="result">The result to serialize.</param>
    /// <returns>A byte array representing the serialized result.</returns>
    public static byte[] SerializeWithMessagePack<T>(this Result<T> result) where T : notnull =>
        MessagePackSerializer.Serialize(result, Options);

    /// <summary>
    ///     Deserializes a byte array back into a Result{T} instance using MessagePack.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <param name="serializedResult">The byte array representing the serialized result.</param>
    /// <returns>The deserialized Result{T} instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the serializedResult is null or empty.</exception>
    public static Result<T> DeserializeWithMessagePack<T>(byte[] serializedResult) where T : notnull
    {
        if (serializedResult == null || serializedResult.Length == 0)
            throw new ArgumentNullException(nameof(serializedResult), "Serialized result cannot be null or empty.");

        return MessagePackSerializer.Deserialize<Result<T>>(serializedResult, Options);
    }

    /// <summary>
    ///     Wraps a <see cref="Result" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an asynchronous
    ///     method.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="Result" />.</returns>
    public static Task<Result> ToTask(this Result result) => Task.FromResult(result);

    /// <summary>
    ///     Wraps a <see cref="Result{T}" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an asynchronous
    ///     method.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the <see cref="Result{T}" />.</typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="Result{T}" />.</returns>
    public static Task<Result<T>> ToTask<T>(this Result<T> result) where T : notnull => Task.FromResult(result);

    /// <summary>
    ///     Wraps a <see cref="ResultWithPayload{T}" /> in a <see cref="Task{TResult}" /> to allow it to be returned from an
    ///     asynchronous method.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated by the <see cref="ResultWithPayload{T}" />.</typeparam>
    /// <param name="result">The <see cref="ResultWithPayload{T}" /> to wrap in a <see cref="Task{TResult}" />.</param>
    /// <returns>A <see cref="Task{TResult}" /> containing the provided <see cref="ResultWithPayload{T}" />.</returns>
    public static Task<ResultWithPayload<T>> ToTask<T>(this ResultWithPayload<T> result) where T : notnull =>
        Task.FromResult(result);


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

        var compressionOptions = Options.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Serialize(result, compressionOptions);
    }

    /// <summary>
    ///     Serializes the given value using MessagePack with adaptive compression. The method initially
    ///     serializes the value without compression to check the serialized data size. If the size exceeds
    ///     a predefined threshold (e.g., 1KB), it serializes the value again with LZ4 block array compression.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>
    ///     A byte array containing the serialized data. The data may be compressed if its
    ///     uncompressed size exceeds the threshold.
    /// </returns>
    /// <remarks>
    ///     This method aims to optimize the trade-off between serialization size and the overhead of compression.
    ///     It uses a simple size threshold to decide when to apply compression, which can be particularly beneficial
    ///     for larger data sizes where compression yields significant size reductions.
    /// </remarks>
    public static byte[] SerializeWithAdaptiveCompression<T>(T value) where T : notnull
    {
        var uncompressed = MessagePackSerializer.Serialize(value, MessagePackSerializerOptions.Standard);
        if (uncompressed.Length > 4096) // Threshold of 4KB
            return MessagePackSerializer.Serialize(value,
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
        return uncompressed;
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

        var compressionOptions = Options.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Deserialize<Result<T>>(compressedData, compressionOptions);
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
        return MessagePackSerializer.Serialize(result, Options);
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

        var result = MessagePackSerializer.Deserialize<ResultWithPayload<T>>(serializedData, Options);

        if (result.Payload != null && !result.Payload.VerifyIntegrity())
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
    public static IActionResult ToActionResult<T>(this ResultWithPayload<T> result) where T : notnull =>
        result.IsSuccess switch
        {
            true when result.Payload != null => new OkObjectResult(result.Payload),
            true => new NoContentResult(),
            _ => new ObjectResult(result.ErrorMessage) { StatusCode = StatusCodes.Status400BadRequest }
        };

    /// <summary>
    ///     Converts a <see cref="Result{T1, T2}" /> to a <see cref="Task{Result{T1, T2}}" /> for use with asynchronous
    ///     operations.
    /// </summary>
    /// <typeparam name="T1">The type of the success value.</typeparam>
    /// <typeparam name="T2">The type of the failure value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>A task wrapping the provided result.</returns>
    public static Task<Result<T1, T2>> ToTask<T1, T2>(this Result<T1, T2> result) => Task.FromResult(result);


    /// <summary>
    ///     Converts a <see cref="Result{T1, T2}" /> to an <see cref="IActionResult" /> suitable for returning from an ASP.NET
    ///     Core controller.
    /// </summary>
    /// <typeparam name="T1">The type of the success value.</typeparam>
    /// <typeparam name="T2">The type of the failure value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An <see cref="IActionResult" /> representing the HTTP response.</returns>
    public static IActionResult ToActionResult<T1, T2>(this Result<T1, T2> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.SuccessValue);
        // Adjust the failure handling as needed (BadRequest, NotFound, etc.)
        return new BadRequestObjectResult(result.FailureValue);
    }
}
