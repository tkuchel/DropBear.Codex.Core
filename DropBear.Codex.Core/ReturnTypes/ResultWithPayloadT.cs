using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes;

using System;

/// <summary>
/// Represents a specialized result type that incorporates a payload, including data, checksum, and optional metadata, 
/// for operations that require data integrity verification alongside the operation result.
/// </summary>
/// <typeparam name="T">The type of data encapsulated as a payload.</typeparam>
[MessagePackObject]
public class ResultWithPayload<T> where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the ResultWithPayload class.
    /// </summary>
    /// <param name="payload">The payload containing the operation's data.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errorMessage">The error message associated with a failed operation.</param>
    private ResultWithPayload(Payload<T>? payload, bool isSuccess, string errorMessage = "")
    {
        Payload = payload;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the payload containing the operation's data. Can be null for failed operations.
    /// </summary>
    [Key(1)]
    public Payload<T>? Payload { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    [IgnoreMember]
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the error message associated with a failed operation.
    /// </summary>
    [Key(0)]
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// Creates a successful result with the specified data, encapsulating it within a Payload that includes a checksum.
    /// </summary>
    /// <param name="data">The data to be included in the result.</param>
    /// <returns>A successful result containing the payload.</returns>
    public static ResultWithPayload<T> Success(T data)
    {
        var payload = Payload<T>.Create(data);
        return new ResultWithPayload<T>(payload, true);
    }

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message for the failure.</param>
    /// <returns>A failure result without a payload.</returns>
    public static ResultWithPayload<T> Failure(string errorMessage)
    {
        return new ResultWithPayload<T>(null, false, errorMessage);
    }

    /// <summary>
    /// Validates the integrity of the data by comparing the computed checksum with the one stored in the payload.
    /// This method can only be called on results with a payload.
    /// </summary>
    /// <returns>True if the checksums match, indicating the data has not been tampered with; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a result without a payload.</exception>
    public bool ValidateChecksum()
    {
        if (Payload == null)
        {
            throw new InvalidOperationException("Cannot validate checksum for a result without a payload.");
        }

        return Payload.ValidateChecksum();
    }
    
}
