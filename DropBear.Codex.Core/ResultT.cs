#region

using System.Collections;
using System.Collections.ObjectModel;

#endregion

namespace DropBear.Codex.Core;

public class Result<T> : Result, IEquatable<Result<T>>, IEnumerable<T>
{
    private Result(T value, string? error, Exception? exception, ResultState state)
        : base(state, error, exception)
    {
        Value = value;
    }

    public T Value { get; }

    public IEnumerator<T> GetEnumerator()
    {
        if (IsSuccess)
        {
            yield return Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(Result<T>? other)
    {
        return base.Equals(other) && EqualityComparer<T>.Default.Equals(Value, other!.Value);
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value, string.Empty, null, ResultState.Success);
    }

    public static new Result<T> Failure(string error, Exception? exception = null)
    {
        return new Result<T>(default!, error, exception, ResultState.Failure);
    }

    public static Result<T> Failure(Exception exception)
    {
        return new Result<T>(default!, exception.Message, exception, ResultState.Failure);
    }

    public static Result<T> Failure(IEnumerable<Exception> exceptions)
    {
        var exceptionList = exceptions.ToList();
        var errorMessage = exceptionList.Count > 0 ? exceptionList[0].Message : "Multiple errors occurred.";
        return new Result<T>(default!, errorMessage, exceptionList.FirstOrDefault(), ResultState.Failure)
        {
            Exceptions = new ReadOnlyCollection<Exception>(exceptionList)
        };
    }

    public static Result<T> Try(Func<T> func)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    public T ValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value : defaultValue;
    }

    public T ValueOrThrow(string? errorMessage = null)
    {
        if (IsSuccess)
        {
            return Value;
        }

        throw new InvalidOperationException(
            errorMessage ?? ErrorMessage ?? "Operation failed without an error message.");
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        return IsSuccess ? func(Value) : Result<TOut>.Failure(ErrorMessage, Exception);
    }

    public new Result OnSuccess(Action action)
    {
        base.OnSuccess(action);
        return this;
    }

    public Result OnSuccess(Func<T, Result> action)
    {
        return IsSuccess ? SafeExecute(() => action(Value)) : this;
    }

    public Result<TOut> OnSuccess<TOut>(Func<T, Result<TOut>> func)
    {
        return IsSuccess ? SafeExecute(() => func(Value)) : Result<TOut>.Failure(ErrorMessage, Exception);
    }

    public new Result<T> OnFailure(Action<string, Exception?> action)
    {
        base.OnFailure(action);
        return this;
    }

    public new TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string, Exception?, TResult> onFailure,
        Func<string, TResult>? onWarning = null,
        Func<string, TResult>? onPartialSuccess = null,
        Func<string, TResult>? onCancelled = null,
        Func<string, TResult>? onPending = null,
        Func<string, TResult>? onNoOp = null)
    {
        return base.Match(onSuccess, onFailure, onWarning, onPartialSuccess, onCancelled, onPending, onNoOp);
    }

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, Exception?, TResult> onFailure,
        Func<string, TResult>? onWarning = null,
        Func<string, TResult>? onPartialSuccess = null,
        Func<string, TResult>? onCancelled = null,
        Func<string, TResult>? onPending = null,
        Func<string, TResult>? onNoOp = null)
    {
        return base.Match(
            () => onSuccess(Value),
            onFailure,
            onWarning,
            onPartialSuccess,
            onCancelled,
            onPending,
            onNoOp);
    }

    public string UnwrapError(string defaultError = "")
    {
        return IsSuccess ? defaultError : ErrorMessage ?? "An unknown error has occurred.";
    }

    public new Result<T> Unwrap()
    {
        return this;
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess ? Result<TOut>.Success(mapper(Value)) : Result<TOut>.Failure(ErrorMessage, Exception);
    }

    private static Result SafeExecute(Func<Result> action)
    {
        try { return action(); }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing action: {ex.Message}");
            return Failure(ex.Message);
        }
    }

    private static Result<TOut> SafeExecute<TOut>(Func<Result<TOut>> func)
    {
        try { return func(); }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing function: {ex.Message}");
            return Result<TOut>.Failure(ex.Message);
        }
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Result<T>);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public static implicit operator Result<T>(Exception exception)
    {
        return Failure(exception);
    }
}
