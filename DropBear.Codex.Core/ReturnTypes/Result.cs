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
    public Result(ExitCode? exitCode, string? errorMessage)
    {
        ExitCode = exitCode ?? throw new ArgumentNullException(nameof(exitCode));
        ErrorMessage = errorMessage ?? string.Empty;
    }


    /// <summary>
    ///     Gets the exit code representing the outcome of the operation.
    /// </summary>
    [Key(0)]
    public ExitCode ExitCode { get; } = null!;

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
    public string ErrorMessage { get; } = null!;

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    /// <returns>A success result.</returns>
    public static Result Success()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return new Result(StandardExitCodes.Success, "");
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    ///     Creates a failure result with the specified error message and exit code.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <returns>A failure result.</returns>
    /// <exception cref="ArgumentException">Thrown when the error message is null or whitespace.</exception>
    public static Result Failure(string? errorMessage, ExitCode? exitCode = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(errorMessage));

#pragma warning disable CS0618 // Type or member is obsolete
        return new Result(exitCode ?? StandardExitCodes.GeneralError, errorMessage);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    private bool Equals(Result other) =>
        // Specify StringComparison for string comparisons for clarity and correctness
        ExitCode == other.ExitCode && string.Equals(ErrorMessage, other.ErrorMessage, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = (int)2166136261;
            // Use StringComparer for generating hash code to align with specified StringComparison
            hash = (hash * 16777619) ^ ExitCode.GetHashCode();
            hash = (hash * 16777619) ^ StringComparer.Ordinal.GetHashCode(ErrorMessage);
            return hash;
        }
    }


    public static bool operator ==(Result left, Result right) => Equals(left, right);

    public static bool operator !=(Result left, Result right) => !Equals(left, right);
}
