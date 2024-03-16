using DropBear.Codex.Core.Models;
using DropBear.Codex.Core.ReturnTypes;

namespace DropBear.Codex.Core.Factories;

/// <summary>
///     Factory for creating instances of ResultWithPayload.
/// </summary>
public static class ResultWithPayloadFactory
{
    /// <summary>
    ///     Creates a success ResultWithPayload containing a fully specified Payload.
    /// </summary>
    public static ResultWithPayload<T> Success<T>(T data, byte[] checksum, byte[] signature, byte[] publicKey,
        long timestamp, string dataType, Guid payloadId) where T : notnull
    {
        var payload = new Payload<T>(data, checksum, signature, publicKey, timestamp, dataType, payloadId);
        return new ResultWithPayload<T>(isSuccess: true, payload, string.Empty);
    }

    /// <summary>
    ///     Creates a success ResultWithPayload using PayloadFactory to generate the payload.
    /// </summary>
    public static ResultWithPayload<T> Success<T>(T data) where T : notnull
    {
        try
        {
            var payload = PayloadFactory.Create(data);
            return new ResultWithPayload<T>(isSuccess: true, payload, string.Empty);
        }
        catch (Exception ex)
        {
            // Depending on how you wish to handle this, logging the exception or handling it differently might be necessary.
            throw new InvalidOperationException("Failed to create Payload", ex);
        }
    }

    /// <summary>
    ///     Creates a failure ResultWithPayload with the specified error message.
    /// </summary>
    public static ResultWithPayload<T> Failure<T>(string errorMessage) where T : notnull =>
        new(isSuccess: false, payload: null, errorMessage);
}
