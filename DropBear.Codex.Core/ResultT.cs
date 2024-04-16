using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

#pragma warning disable MA0048
public class Result<T> : IEquatable<Result<T>>
#pragma warning restore MA0048
{

    internal Result(T value, string error, Exception? exception, ResultState state)
    {
        Value = value;
        Error = error ?? string.Empty;
        Exception = exception;
        State = state;
    }

    public T Value { get; }
    public string Error { get; }
    public Exception? Exception { get; }
    public ResultState State { get; }

    public bool Equals(Result<T>? other)
    {
        if (other == null) return false;
        // Using State instead of IsSuccess
        return State == other.State && EqualityComparer<T>.Default.Equals(Value, other.Value) && Error == other.Error;
    }

    [Pure]
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) => State is ResultState.Failure
        ? ResultFactory.Failure<TOut>(Error, Exception)
        : func(Value);

    [Pure]
    public Result<T> OnSuccess(Action<T> action)
    {
        if (State is ResultState.Success) action(Value);
        return this;
    }

    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> func)
    {
        if (State is ResultState.Failure) return ResultFactory.Failure<TOut>(Error, Exception);
        return await func(Value).ConfigureAwait(false);
    }

    public Result<T> Validate(Func<T, bool> predicate, string failureMessage)
    {
        if (State is ResultState.Success && !predicate(Value))
            return ResultFactory.Failure<T>(failureMessage);
        return this;
    }

    [Pure]
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (State is ResultState.Failure) action(Error, Exception);
        return this;
    }

    [Pure]
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, Exception?, TResult> onFailure) =>
        State is ResultState.Success ? onSuccess(Value) : onFailure(Error, Exception);

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess,
        Func<string, Exception?, Task<TResult>> onFailure) =>
        State is ResultState.Success
            ? await onSuccess(Value).ConfigureAwait(false)
            : await onFailure(Error, Exception).ConfigureAwait(false);

    public async Task<Result<T>> OnSuccessAsync(Func<T, Task> action)
    {
        if (State is ResultState.Success) await action(Value).ConfigureAwait(false);
        return this;
    }

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
