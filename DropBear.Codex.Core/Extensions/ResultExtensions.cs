using System.Reflection;
using DropBear.Codex.Core.Resolvers;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DropBear.Codex.Core.Extensions;

public static class ResultExtensions
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithResolver(CustomResolver.Instance)
        .WithSecurity(MessagePackSecurity.UntrustedData);

    public static void EnsureMessagePackCompatibility<T>()
    {
        var type = typeof(T);
        if (!type.IsPublic || type.IsNested)
            throw new InvalidOperationException($"Type {type.Name} must be public and not nested.");

        var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
        if (messagePackObjectAttribute is null)
            throw new InvalidOperationException($"Type {type.Name} must have a MessagePackObject attribute.");

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var keyAttribute = property.GetCustomAttribute<KeyAttribute>();
            if (keyAttribute is null)
                throw new InvalidOperationException(
                    $"Property {property.Name} in type {type.Name} must have a Key attribute for MessagePack serialization.");
        }
    }

    public static byte[] SerializeWithMessagePackAndCompress<T>(this ResultWithPayload<T> result) where T : notnull
    {
        if (result is null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        var compressionOptions = Options.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Serialize(result, compressionOptions);
    }

    public static ResultWithPayload<T> DeserializeAndDecompressWithMessagePack<T>(byte[] compressedData)
        where T : notnull
    {
        if (compressedData is null || compressedData.Length is 0)
            throw new ArgumentNullException(nameof(compressedData), "Compressed data cannot be null or empty.");

        var compressionOptions = Options.WithCompression(MessagePackCompression.Lz4BlockArray);
        return MessagePackSerializer.Deserialize<ResultWithPayload<T>>(compressedData, compressionOptions);
    }

    /// <summary>
    ///     Serializes a Result T1, T2 to a byte array using MessagePack without compression.
    /// </summary>
    public static byte[] SerializeWithMessagePack<T1, T2>(this Result<T1, T2> result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result), "Result cannot be null.");
        return MessagePackSerializer.Serialize(result, Options);
    }

    /// <summary>
    ///     Deserializes a byte array back into a Result T1, T2 instance using MessagePack without compression.
    /// </summary>
    public static Result<T1, T2> DeserializeWithMessagePack<T1, T2>(byte[] serializedResult)
    {
        if (serializedResult is null || serializedResult.Length is 0)
            throw new ArgumentNullException(nameof(serializedResult), "Serialized result cannot be null or empty.");

        return MessagePackSerializer.Deserialize<Result<T1, T2>>(serializedResult, Options);
    }

    public static IActionResult ToActionResult<T>(this ResultWithPayload<T> result) where T : notnull
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result), "Result cannot be null.");

        return result.IsSuccess switch
        {
            true when result.Payload is not null => new OkObjectResult(result.Payload),
            true => new NoContentResult(),
            _ => new BadRequestObjectResult(result.ErrorMessage) { StatusCode = StatusCodes.Status400BadRequest }
        };
    }

    public static Task<ResultWithPayload<T>> ToTask<T>(this ResultWithPayload<T> result) where T : notnull =>
        Task.FromResult(result);

    /// <summary>
    ///     Adapts a byte array to an async Task for easy deserialization in an asynchronous context.
    /// </summary>
    public static async Task<ResultWithPayload<T>> DeserializeAsync<T>(byte[] data) where T : notnull =>
        await Task.Run(() => DeserializeAndDecompressWithMessagePack<T>(data)).ConfigureAwait(false);

    /// <summary>
    ///     Facilitates the asynchronous serialization of a ResultWithPayload instance.
    /// </summary>
    public static async Task<byte[]> SerializeAsync<T>(ResultWithPayload<T> result) where T : notnull =>
        await Task.Run(() => SerializeWithMessagePackAndCompress(result)).ConfigureAwait(false);
}
