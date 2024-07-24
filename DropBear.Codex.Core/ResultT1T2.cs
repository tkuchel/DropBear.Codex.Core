#region

namespace DropBear.Codex.Core;

#endregion

#pragma warning disable MA0048
public class Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
#pragma warning restore MA0048
{
    private readonly TFailure _failure;
    private readonly TSuccess _success;

    private Result(TSuccess success, TFailure failure, ResultState state)
    {
        _success = success;
        _failure = failure;
        State = state;
    }

    public ResultState State { get; }
    public bool IsSuccess => State == ResultState.Success;

    public TSuccess Success =>
        IsSuccess ? _success : throw new InvalidOperationException("Cannot access Success on a failed result.");

    public TFailure Failure => !IsSuccess
        ? _failure
        : throw new InvalidOperationException("Cannot access Failure on a successful result.");

    public bool Equals(Result<TSuccess, TFailure>? other)
    {
        return other is not null && State == other.State &&
               EqualityComparer<TSuccess>.Default.Equals(_success, other._success) &&
               EqualityComparer<TFailure>.Default.Equals(_failure, other._failure);
    }

#pragma warning disable CA1000
    public static Result<TSuccess, TFailure> Succeeded(TSuccess value)

    {
        return new Result<TSuccess, TFailure>(value, default!, ResultState.Success);
    }

    public static Result<TSuccess, TFailure> Failed(TFailure error)
    {
        return new Result<TSuccess, TFailure>(default!, error, ResultState.Failure);
    }
#pragma warning restore CA1000
    public TResult Match<TResult>(
        Func<TSuccess, TResult> onSuccess,
        Func<TFailure, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_success) : onFailure(_failure);
    }

    public void Match(
        Action<TSuccess> onSuccess,
        Action<TFailure> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(_success);
        }
        else
        {
            onFailure(_failure);
        }
    }

    public Result<TNewSuccess, TFailure> Bind<TNewSuccess>(
        Func<TSuccess, Result<TNewSuccess, TFailure>> onSuccess)
    {
        return IsSuccess ? onSuccess(_success) : Result<TNewSuccess, TFailure>.Failed(_failure);
    }

    public Result<TNewSuccess, TFailure> Map<TNewSuccess>(Func<TSuccess, TNewSuccess> mapper)
    {
        return IsSuccess
            ? Result<TNewSuccess, TFailure>.Succeeded(mapper(_success))
            : Result<TNewSuccess, TFailure>.Failed(_failure);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Result<TSuccess, TFailure>);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(State, _success, _failure);
    }

    public static implicit operator Result<TSuccess, TFailure>(TSuccess success)
    {
        return Succeeded(success);
    }

    public static implicit operator Result<TSuccess, TFailure>(TFailure failure)
    {
        return Failed(failure);
    }
}
