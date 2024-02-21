using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Helpers;
using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes;

/// <summary>
///     Represents the result of an operation, encapsulating success or failure.
/// </summary>
[MessagePackObject]
public class Result
{
    /// <summary>
    ///     Initializes a new instance of the Result class for deserialization purposes only.
    ///     Not intended for direct use in code.
    /// </summary>
    [Obsolete("For deserialization purposes only. Not intended for direct use in code.")]
    protected Result()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the Result class with the specified exit code and error message.
    /// </summary>
    /// <param name="exitCode">The exit code representing the outcome of the operation.</param>
    /// <param name="errorMessage">The error message associated with a failure.</param>
    [Obsolete("For MessagePack serialization only. Not intended for direct use in code.", false)]
    public Result(ExitCode exitCode, string errorMessage)
    {
        ExitCode = exitCode ?? throw new ArgumentNullException(nameof(exitCode));
        ErrorMessage = errorMessage ?? string.Empty;
    }


    /// <summary>
    ///     Gets the exit code representing the outcome of the operation.
    /// </summary>
    [Key(0)]
    public ExitCode ExitCode { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    [IgnoreMember]
    public bool IsSuccess => ExitCode == StandardExitCodes.Success;

    /// <summary>
    ///     Gets a value indicating whether the operation failed.
    /// </summary>
    [IgnoreMember]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the error message associated with a failure.
    /// </summary>
    [Key(1)]
    public string ErrorMessage { get; }

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    /// <returns>A success result.</returns>
    public static Result Success()
    {
        return new Result(StandardExitCodes.Success,"");
    }

    /// <summary>
    ///     Creates a failure result with the specified error message and exit code.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <returns>A failure result.</returns>
    /// <exception cref="ArgumentException">Thrown when the error message is null or whitespace.</exception>
    public static Result Failure(string errorMessage, ExitCode? exitCode = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(errorMessage));

        return new Result(exitCode ?? StandardExitCodes.GeneralError, errorMessage);
    }

    public override bool Equals(object? obj)
    {
        return obj is Result other && Equals(other);
    }

    protected bool Equals(Result other)
    {
        return ExitCode == other.ExitCode && ErrorMessage == other.ErrorMessage;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = (int)2166136261;
            hash = (hash * 16777619) ^ ExitCode.GetHashCode();
            hash = (hash * 16777619) ^ (ErrorMessage?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public static bool operator ==(Result left, Result right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Result left, Result right)
    {
        return !Equals(left, right);
    }
}