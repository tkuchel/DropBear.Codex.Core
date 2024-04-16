using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation, indicating success or failure along with optional error information.
/// </summary>
public class Result : IEquatable<Result>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result" /> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error message if the operation failed.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    protected Result(bool isSuccess, string error, Exception? exception)
    {
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new ArgumentException("An error message must be provided for failure results.", nameof(error));

        IsSuccess = isSuccess;
        Error = error ?? string.Empty; // Ensure error is never null
        Exception = exception;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets the error message if the operation failed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    ///     Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Equals(Result? other)
    {
        if (other is null) return false;
        return IsSuccess == other.IsSuccess && Error == other.Error && Equals(Exception, other.Exception);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Result" /> representing a successful operation.
    /// </summary>
    /// <returns>A new <see cref="Result" /> instance indicating success.</returns>
    [Pure]
    public static Result Success() => new(true, string.Empty, null);

    /// <summary>
    ///     Creates a new instance of <see cref="Result" /> representing a failed operation.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <returns>A new <see cref="Result" /> instance indicating failure.</returns>
    [Pure]
    public static Result Failure(string error, Exception? exception = null)
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(error));

        return new Result(false, error, exception);
    }

    /// <summary>
    ///     Executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void OnSuccess(Action action)
    {
        if (IsSuccess) action();
    }

    /// <summary>
    ///     Executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The action to execute, taking the error message and exception.</param>
    public void OnFailure(Action<string, Exception?> action)
    {
        if (!IsSuccess) action(Error, Exception);
    }

    /// <summary>
    ///     Executes the specified action regardless of the operation's success or failure.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void OnBoth(Action<Result> action) => action(this);

    /// <summary>
    ///     Asynchronously executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    public async Task OnSuccessAsync(Func<Task> action)
    {
        if (IsSuccess) await action().ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The asynchronous action to execute, taking the error message and exception.</param>
    public async Task OnFailureAsync(Func<string, Exception?, Task> action)
    {
        if (!IsSuccess) await action(Error, Exception).ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously executes the specified action regardless of the operation's success or failure.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    public async Task OnBothAsync(Func<Result, Task> action) => await action(this).ConfigureAwait(false);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Result);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(IsSuccess, Error, Exception);
}
