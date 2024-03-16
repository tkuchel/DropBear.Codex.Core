using DropBear.Codex.Core.Resolvers;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using Microsoft.AspNetCore.Mvc;

namespace DropBear.Codex.Core.Extensions;

public static class ResultExtensions
{
    #region Field and Property Definitions

    private static readonly MessagePackSerializerOptions DefinedMessagePackSerializerOptions =
        MessagePackSerializerOptions.Standard
            .WithResolver(CustomResolver.Instance)
            .WithSecurity(MessagePackSecurity.UntrustedData);

    #endregion

    #region Public Methods

    /// <summary>
    ///     Serializes a Result T1, T2 to a byte array using MessagePack with LZ4 compression.
    /// </summary>
    /// <typeparam name="T1">The type of the first result value.</typeparam>
    /// <typeparam name="T2">The type of the second result value.</typeparam>
    /// <param name="result">The Result T1, T2 object to serialize.</param>
    /// <returns>A byte array containing the serialized and compressed Result T1, T2.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static byte[] SerializeWithMessagePackAndCompress<T1, T2>(this Result<T1, T2> result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        // Specify the compression method to use. LZ4BlockArray is chosen here for its balance between compression
        // efficiency and speed. Consider evaluating different compression options based on the specific use case's
        // performance and compression ratio requirements.
        var compressionOptions =
            DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

        // Serialize the object with the specified options. The generic type parameter is inferred from the method's
        // type parameters, ensuring that the serialization process respects the actual types of T1 and T2.
        // Note: It's important to handle any potential exceptions that could arise during serialization, such as
        // MessagePackSerializationException, to ensure the calling code can react appropriately.
        try
        {
            return MessagePackSerializer.Serialize(result, compressionOptions);
        }
        catch (MessagePackSerializationException ex)
        {
            // Consider logging the exception details here and/or rethrowing a more specific exception to the caller.
            throw new InvalidOperationException("Failed to serialize the result object.", ex);
        }
    }

    /// <summary>
    ///     Serializes a Result T to a byte array using MessagePack with LZ4 compression.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated in the Result object, must not be null.</typeparam>
    /// <param name="result">The Result T object to serialize.</param>
    /// <returns>A byte array containing the serialized and compressed Result T.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static byte[] SerializeWithMessagePackAndCompress<T>(this Result<T> result) where T : notnull
    {
        if (result is null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        // Specify the compression method to use. LZ4BlockArray is chosen here for its balance between compression
        // efficiency and speed. Consider evaluating different compression options based on your specific use case's
        // performance and compression ratio requirements.
        var compressionOptions =
            DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

        // Serialize the object with the specified options. The generic type parameter ensures that the serialization
        // process respects the actual type of T.
        // Note: It's important to handle any potential exceptions that could arise during serialization, such as
        // MessagePackSerializationException, to ensure the calling code can react appropriately.
        try
        {
            return MessagePackSerializer.Serialize(result, compressionOptions);
        }
        catch (MessagePackSerializationException ex)
        {
            // Consider logging the exception details here and/or rethrowing a more specific exception to the caller.
            throw new InvalidOperationException("Failed to serialize the result object.", ex);
        }
    }

    /// <summary>
    ///     Serializes a Result to a byte array using MessagePack with LZ4 compression.
    /// </summary>
    /// <param name="result">The Result object to serialize.</param>
    /// <returns>A byte array containing the serialized and compressed Result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static byte[] SerializeWithMessagePackAndCompress(this Result result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        // Specify the compression method to use. LZ4BlockArray is chosen here for its balance between compression
        // efficiency and speed. Consider evaluating different compression options based on your specific use case's
        // performance and compression ratio requirements.
        var compressionOptions =
            DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

        // Serialize the object with the specified options. 
        // Note: It's important to handle any potential exceptions that could arise during serialization, such as
        // MessagePackSerializationException, to ensure the calling code can react appropriately.
        try
        {
            return MessagePackSerializer.Serialize(result, compressionOptions);
        }
        catch (MessagePackSerializationException ex)
        {
            // Consider logging the exception details here and/or rethrowing a more specific exception to the caller.
            throw new InvalidOperationException("Failed to serialize the result object.", ex);
        }
    }

    /// <summary>
    ///     Serializes a ResultWithPayload T to a byte array using MessagePack with LZ4 compression.
    /// </summary>
    /// <typeparam name="T">The type of the payload encapsulated in the ResultWithPayload object, must not be null.</typeparam>
    /// <param name="resultWithPayload">The ResultWithPayload T object to serialize.</param>
    /// <returns>A byte array containing the serialized and compressed ResultWithPayload T.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the resultWithPayload argument is null.</exception>
    public static byte[] SerializeWithMessagePackAndCompress<T>(this ResultWithPayload<T> resultWithPayload)
        where T : notnull
    {
        if (resultWithPayload is null)
            throw new ArgumentNullException(nameof(resultWithPayload), "ResultWithPayload cannot be null.");

        // Specify the compression method to use. LZ4BlockArray is chosen here for its balance between compression
        // efficiency and speed. Consider evaluating different compression options based on your specific use case's
        // performance and compression ratio requirements.
        var compressionOptions =
            DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

        // Serialize the object with the specified options. The generic type parameter ensures that the serialization
        // process respects the actual type of T.
        // Note: It's important to handle any potential exceptions that could arise during serialization, such as
        // MessagePackSerializationException, to ensure the calling code can react appropriately.
        try
        {
            return MessagePackSerializer.Serialize(resultWithPayload, compressionOptions);
        }
        catch (MessagePackSerializationException ex)
        {
            // Consider logging the exception details here and/or rethrowing a more specific exception to the caller.
            throw new InvalidOperationException("Failed to serialize the ResultWithPayload object.", ex);
        }
    }

    /// <summary>
    ///     Deserializes and decompresses a byte array to an object of type T using MessagePack.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="compressedBytes">The compressed byte array.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the compressedBytes argument is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
    public static T DeserializeAndDecompressWithMessagePack<T>(this byte[] compressedBytes) where T : notnull
    {
        if (compressedBytes is null)
            throw new ArgumentNullException(nameof(compressedBytes), "Compressed bytes cannot be null.");

        // Specify the options to use for decompression. Assuming LZ4 compression was used during serialization.
        var options = DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

        try
        {
            // Deserialize the object with the specified options. The generic type parameter T ensures that the
            // deserialization process respects the actual type.
            return MessagePackSerializer.Deserialize<T>(compressedBytes, options);
        }
        catch (MessagePackSerializationException ex)
        {
            // Consider logging the exception details here and/or rethrowing a more specific exception to the caller.
            throw new InvalidOperationException("Failed to deserialize the compressed bytes.", ex);
        }
    }

    /// <summary>
    ///     Asynchronously serializes and compresses a Result object using MessagePack with LZ4 compression.
    /// </summary>
    /// <param name="result">The Result object to serialize and compress.</param>
    /// <returns>A task that represents the asynchronous operation and contains the compressed byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static async Task<byte[]> SerializeWithMessagePackAndCompressAsync(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
        var stream = new MemoryStream();
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerializer.SerializeAsync(stream, result, options).ConfigureAwait(false);
            return stream.ToArray();
        }
    }

    /// <summary>
    ///     Asynchronously serializes and compresses a Result&lt;T&gt; object using MessagePack with LZ4 compression.
    /// </summary>
    /// <typeparam name="T">The type of the value encapsulated in the Result object.</typeparam>
    /// <param name="result">The Result&lt;T&gt; object to serialize and compress.</param>
    /// <returns>A task that represents the asynchronous operation and contains the compressed byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static async Task<byte[]> SerializeWithMessagePackAndCompressAsync<T>(Result<T> result) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
        var stream = new MemoryStream();
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerializer.SerializeAsync(stream, result, options).ConfigureAwait(false);
            return stream.ToArray();
        }
    }

    /// <summary>
    ///     Asynchronously serializes and compresses a ResultWithPayload&lt;T&gt; object using MessagePack with LZ4
    ///     compression.
    /// </summary>
    /// <typeparam name="T">The type of the payload encapsulated in the ResultWithPayload object.</typeparam>
    /// <param name="result">The ResultWithPayload&lt;T&gt; object to serialize and compress.</param>
    /// <returns>A task that represents the asynchronous operation and contains the compressed byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the result argument is null.</exception>
    public static async Task<byte[]> SerializeWithMessagePackAndCompressAsync<T>(ResultWithPayload<T> result)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
        var stream = new MemoryStream();
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerializer.SerializeAsync(stream, result, options).ConfigureAwait(false);
            return stream.ToArray();
        }
    }

    /// <summary>
    ///     Asynchronously deserializes and decompresses a byte array to an object of type T using MessagePack with LZ4
    ///     decompression.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="compressedBytes">The compressed byte array to deserialize and decompress.</param>
    /// <returns>A task that represents the asynchronous operation and contains the deserialized object of type T.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the compressedBytes argument is null.</exception>
    public static async Task<T> DeserializeAndDecompressWithMessagePackAsync<T>(byte[] compressedBytes)
    {
        ArgumentNullException.ThrowIfNull(compressedBytes);

        var options = DefinedMessagePackSerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
        var stream = new MemoryStream(compressedBytes);
        await using (stream.ConfigureAwait(false))
        {
            return await MessagePackSerializer.DeserializeAsync<T>(stream, options).ConfigureAwait(false);
        }
    }


    // Converts Result to IActionResult
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();
        return
            new ObjectResult(result.ErrorMessage) { StatusCode = 400 };
    }

    // Converts Result<T> to IActionResult<T>
    public static IActionResult ToActionResult<T>(this Result<T> result) where T : notnull =>
        result.IsSuccess
            ? new OkObjectResult(result.Value)
            : new ObjectResult(result.ErrorMessage) { StatusCode = 400 }; // Adjust status code as needed

    // Converts ResultWithPayload<T> to IActionResult<T>
    public static IActionResult ToActionResult<T>(this ResultWithPayload<T> result) where T : notnull
    {
        return result is { IsSuccess: true, Payload: not null } ? new OkObjectResult(result.Payload) : new ObjectResult(result.ErrorMessage) { StatusCode = 400 }; // Adjust status code as needed
    }

    // Async overloads or ToTask methods for async contexts
    public static async Task<IActionResult> ToActionResultAsync(this Task<Result> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.ToActionResult();
    }

    public static async Task<IActionResult> ToActionResultAsync<T>(this Task<Result<T>> task) where T : notnull
    {
        var result = await task.ConfigureAwait(false);
        return result.ToActionResult();
    }

    public static async Task<IActionResult> ToActionResultAsync<T>(this Task<ResultWithPayload<T>> task)
        where T : notnull
    {
        var result = await task.ConfigureAwait(false);
        return result.ToActionResult();
    }

    // Assuming a Result<T1, T2> class exists, this method would need to be adjusted to fit its actual implementation.
    public static IActionResult ToActionResult<T1, T2>(this Result<T1, T2> result) =>
        result.IsSuccess
            ? new OkObjectResult(result.SuccessValue)
            : new ObjectResult(result.FailureValue) { StatusCode = 400 }; // Adjust as needed.

    // Async version for Result<T1, T2>
    public static async Task<IActionResult> ToActionResultAsync<T1, T2>(this Task<Result<T1, T2>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.ToActionResult(); // Assuming ToActionResult<T1, T2> is correctly implemented for Result<T1, T2>.
    }

    #endregion

    #region Constructors

    #endregion

    #region Protected and Internal Methods

    #endregion

    #region Private Methods

    #endregion

    #region Events and Handlers

    #endregion

    #region Overrides

    #endregion

    #region Nested Classes

    #endregion
}
