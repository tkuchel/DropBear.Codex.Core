using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation that may return a value of type <typeparamref name="T" />,
///     indicating success or failure along with optional error information.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
#pragma warning disable MA0048
public class Result<T> : IEquatable<Result<T>>
#pragma warning restore MA0048
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{T}" /> class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="error">The error message if the operation failed.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="state">The state of the result (Success or Failure).</param>
    internal Result(T value, string error, Exception? exception, ResultState state)
    {
        Value = value;
        Error = error ?? string.Empty;
        Exception = exception;
        State = state;
    }

    /// <summary>
    ///     Gets the result value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///     Gets the error message if the operation failed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    ///     Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    ///     Gets the state of the result (Success or Failure).
    /// </summary>
    public ResultState State { get; }

    /// <inheritdoc />
    public bool Equals(Result<T>? other)
    {
        if (other == null) return false;
        return State == other.State && EqualityComparer<T>.Default.Equals(Value, other.Value) && Error == other.Error;
    }

    /// <summary>
    ///     Binds the result to another operation.
    /// </summary>
    /// <typeparam name="TOut">The type of the result value produced by the binding operation.</typeparam>
    /// <param name="func">The function to apply to the current result value.</param>
    /// <returns>The result of applying the function to the current result value.</returns>
    [Pure]
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) => State is ResultState.Failure
        ? ResultFactory.Failure<TOut>(Error, Exception)
        : func(Value);

    /// <summary>
    ///     Executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The action to execute if the operation was successful.</param>
    /// <returns>The current <see cref="Result{T}" /> instance.</returns>
    [Pure]
    public Result<T> OnSuccess(Action<T> action)
    {
        if (State is ResultState.Success) action(Value);
        return this;
    }

    /// <summary>
    ///     Asynchronously binds the result to another operation.
    /// </summary>
    /// <typeparam name="TOut">The type of the result value produced by the asynchronous binding operation.</typeparam>
    /// <param name="func">The asynchronous function to apply to the current result value.</param>
    /// <returns>The result of applying the asynchronous function to the current result value.</returns>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> func)
    {
        if (State is ResultState.Failure) return ResultFactory.Failure<TOut>(Error, Exception);
        return await func(Value).ConfigureAwait(false);
    }

    /// <summary>
    ///     Validates the result using the specified predicate and failure message.
    /// </summary>
    /// <param name="predicate">The predicate to validate the result value.</param>
    /// <param name="failureMessage">The error message to use if the validation fails.</param>
    /// <returns>The current <see cref="Result{T}" /> instance.</returns>
    public Result<T> Validate(Func<T, bool> predicate, string failureMessage)
    {
        if (State is ResultState.Success && !predicate(Value))
            return ResultFactory.Failure<T>(failureMessage);
        return this;
    }

    /// <summary>
    ///     Executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The action to execute if the operation failed.</param>
    /// <returns>The current <see cref="Result{T}" /> instance.</returns>
    [Pure]
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (State is ResultState.Failure) action(Error, Exception);
        return this;
    }

    /// <summary>
    ///     Matches the result with the specified functions, depending on whether it's a success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">The function to apply if the operation was successful.</param>
    /// <param name="onFailure">The function to apply if the operation failed.</param>
    /// <returns>The result of applying the corresponding function based on the result state.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, Exception?, TResult> onFailure) =>
        State is ResultState.Success ? onSuccess(Value) : onFailure(Error, Exception);

    /// <summary>
    ///     Asynchronously matches the result with the specified asynchronous functions, depending on whether it's a success or
    ///     failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return asynchronously.</typeparam>
    /// <param name="onSuccess">The asynchronous function to apply if the operation was successful.</param>
    /// <param name="onFailure">The asynchronous function to apply if the operation failed.</param>
    /// <returns>The result of applying the corresponding asynchronous function based on the result state.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess,
        Func<string, Exception?, Task<TResult>> onFailure) =>
        State is ResultState.Success
            ? await onSuccess(Value).ConfigureAwait(false)
            : await onFailure(Error, Exception).ConfigureAwait(false);

    /// <summary>
    ///     Asynchronously executes the specified action if the operation was successful.
    /// </summary>
    /// <param name="action">The asynchronous action to execute if the operation was successful.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<T>> OnSuccessAsync(Func<T, Task> action)
    {
        if (State is ResultState.Success) await action(Value).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The asynchronous action to execute if the operation failed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<T>> OnFailureAsync(Func<string, Exception?, Task> action)
    {
        if (State is ResultState.Failure) await action(Error, Exception).ConfigureAwait(false);
        return this;
    }

    // Implicit conversion from T to Result<T> (Success)
    public static implicit operator Result<T>(T value) => ResultFactory.Success(value);

    // Implicit conversion from Exception to Result<T> (Failure)
    public static implicit operator Result<T>(Exception exception) => ResultFactory.Failure<T>(exception);

    public override bool Equals(object? obj) => Equals(obj as Result<T>);
    public override int GetHashCode() => HashCode.Combine(State, Value, Error);
}
