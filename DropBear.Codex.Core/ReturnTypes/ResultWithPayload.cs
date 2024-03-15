using DropBear.Codex.Core.Models;
using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes;

public static class ResultWithPayload
{
    public static ResultWithPayload<T> Success<T>(T data) where T : notnull
    {
        var payload = new Payload<T>(data);
        return new ResultWithPayload<T>(payload, isSuccess: true);
    }

    public static ResultWithPayload<T> Failure<T>(string errorMessage) where T : notnull =>
        new(payload: null, isSuccess: false, errorMessage);
}

/// <summary>
///     Represents a result that may contain a payload or an error message.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
[MessagePackObject]
public class ResultWithPayload<T> where T : notnull
{
    internal ResultWithPayload(Payload<T>? payload, bool isSuccess, string errorMessage = "")
    {
        Payload = payload;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Gets the payload of the result.
    /// </summary>
    [Key(1)]
    public Payload<T>? Payload { get; }

    /// <summary>
    ///     Gets a value indicating whether the result was successful.
    /// </summary>
    [IgnoreMember]
    public bool IsSuccess { get; private set; }

    /// <summary>
    ///     Gets the error message if the result was not successful.
    /// </summary>
    [Key(0)]
    public string ErrorMessage { get; private set; }

    public bool ValidateIntegrity()
    {
        if (Payload is null)
            throw new InvalidOperationException("Cannot validate checksum / signature for a result without a payload.");

        return Payload.VerifyIntegrity();
    }

    public static implicit operator ResultWithPayload<T>(T data) => ResultWithPayload.Success(data);

    public static implicit operator ResultWithPayload<T>(string errorMessage) =>
        ResultWithPayload.Failure<T>(errorMessage);
}
