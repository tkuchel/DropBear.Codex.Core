using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation that may return a value of type T, indicating success or failure along with
///     optional error information.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
#pragma warning disable MA0048
public class Result<T> : IEquatable<Result<T>>
#pragma warning restore MA0048
{
    /// <summary>
    ///     Initializes a new instance of the Result&lt;T&gt; class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="error">The error message if the operation failed.</param>
    /// <param name="exception">The exception that occurred during the operation, if any.</param>
    /// <param name="state">The state of the result (Success or Failure).</param>
    private Result(T value, string? error, Exception? exception, ResultState state)
    {
        if (state is ResultState.Failure or ResultState.PartialSuccess && string.IsNullOrEmpty(error))
            throw new ArgumentException("An error message must be provided for non-success results.", nameof(error));

        Value = value;
        ErrorMessage = error ?? string.Empty;
        Exception = exception;
        State = state;
    }

    public ResultState State { get; }
    public T Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => State is ResultState.Success or ResultState.PartialSuccess;

    public bool Equals(Result<T>? other) =>
        other is not null && State == other.State && EqualityComparer<T>.Default.Equals(Value, other.Value) &&
        ErrorMessage == other.ErrorMessage && Equals(Exception, other.Exception);

    public T ValueOrThrow(string? errorMessage = "")
    {
        if (IsSuccess) return Value;

        throw new InvalidOperationException(
            errorMessage ?? ErrorMessage ?? "Operation failed without an error message.");
    }

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    /// <returns>A successful result with the specified value.</returns>
    [Pure]
#pragma warning disable CA1000
    public static Result<T> Success(T value) => new(value, string.Empty, null, ResultState.Success);
#pragma warning restore CA1000

    /// <summary>
    ///     Creates a failure result with the specified error message and optional exception.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if available.</param>
    /// <returns>A failure result with the specified error message and optional exception.</returns>
    [Pure]
#pragma warning disable CA1000
    public static Result<T> Failure(string error, Exception? exception = null) =>
#pragma warning restore CA1000
        new(default!, error, exception, ResultState.Failure);

    /// <summary>
    ///     Creates a failure result with the specified exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failure result with the specified exception.</returns>
    [Pure]
#pragma warning disable CA1000
    public static Result<T> Failure(Exception exception) =>
#pragma warning restore CA1000
        new(default!, exception.Message, exception, ResultState.Failure);

    /// <summary>
    ///     Binds the result to another operation.
    /// </summary>
    /// <param name="func">The function to apply to the current result value if the operation was successful.</param>
    /// <returns>The result of applying the function to the current result value.</returns>
    [Pure]
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) =>
        State is ResultState.Success ? func(Value) : Result<TOut>.Failure(ErrorMessage ?? $"An unknown error has occured", Exception);

    /// <summary>
    ///     Executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void OnSuccess(Action<T> action)
    {
        if (State is ResultState.Success)
            SafeExecute(() => action(Value));
    }

    /// <summary>
    ///     Executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The action to execute, providing error information and exception if available.</param>
    public void OnFailure(Action<string, Exception?> action)
    {
        if (State is ResultState.Failure)
            SafeExecute(() => action(ErrorMessage ?? "An unknown error has occured.", Exception));
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    public async Task OnSuccessAsync(Func<T, Task> action)
    {
        if (State is ResultState.Success) await SafeExecuteAsync(() => action(Value)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The asynchronous action to execute, providing error information and exception if available.</param>
    public async Task OnFailureAsync(Func<string, Exception?, Task> action)
    {
        if (State is ResultState.Failure)
            await SafeExecuteAsync(() => action(ErrorMessage ?? "An unknown error has occured.", Exception)).ConfigureAwait(false);
    }

    private static void SafeExecute(Action action)
    {
        try { action(); }
        catch (Exception ex)
        {
            // Handle exception, log it, or propagate as needed
            Console.WriteLine("ErrorMessage executing action: " + ex.Message);
        }
    }

    private static async Task SafeExecuteAsync(Func<Task> action)
    {
        try { await action().ConfigureAwait(false); }
        catch (Exception ex)
        {
            // Handle exception, log it, or propagate as needed
            Console.WriteLine("ErrorMessage executing async action: " + ex.Message);
        }
    }

    public override bool Equals(object? obj) => Equals(obj as Result<T>);

    public override int GetHashCode() => HashCode.Combine(State, Value, ErrorMessage, Exception);

    // Implicit conversion from T to Result<T> (Success)
    public static implicit operator Result<T>(T value) => Success(value);

    // Implicit conversion from Exception to Result<T> (Failure)
    public static implicit operator Result<T>(Exception exception) => Failure(exception);
}
