using DropBear.Codex.Core.Models;

namespace DropBear.Codex.Core.ReturnTypes;

public static class ResultWithPayloadFactory
{
    public static ResultWithPayload<T> Success<T>(T data, byte[] checksum, byte[] signature, byte[] publicKey,
        long timestamp, string? dataType, Guid payloadId) where T : notnull
    {
        var payload = new Payload<T>(data, checksum, signature, publicKey, timestamp, dataType, payloadId);
        return new ResultWithPayload<T>(isSuccess: true, payload, string.Empty);
    }

    public static ResultWithPayload<T> Failure<T>(string errorMessage) where T : notnull =>
        new(isSuccess: false, payload: null, errorMessage);
}
