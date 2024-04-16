using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

#pragma warning disable MA0048
public class Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
#pragma warning restore MA0048
{

    // Private constructor to ensure that instances are created only through the factory methods
    internal Result(TSuccess success, TFailure failure, ResultState state)
    {
        Success = success;
        Failure = failure;
        State = state;
    }

    public TSuccess Success { get; }
    public TFailure Failure { get; }
    public ResultState State { get; }

    public bool Equals(Result<TSuccess, TFailure>? other)
    {
        if (other is null) return false;
        // Using State to compare
        return State == other.State && EqualityComparer<TSuccess>.Default.Equals(Success, other.Success) &&
               EqualityComparer<TFailure>.Default.Equals(Failure, other.Failure);
    }

    public static implicit operator Result<TSuccess, TFailure>(TSuccess success) =>
        ResultFactory.Success<TSuccess, TFailure>(success);

    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) =>
        ResultFactory.Failure<TSuccess, TFailure>(failure);

    [Pure]
    public T Match<T>(Func<TSuccess, T> onSuccess, Func<TFailure, T> onFailure) =>
        State switch
        {
            ResultState.Success => onSuccess(Success),
            ResultState.Failure => onFailure(Failure),
            _ => throw new InvalidOperationException("Invalid state")
        };

    public void Match(Action<TSuccess> onSuccess, Action<TFailure> onFailure)
    {
        if (State is ResultState.Success) onSuccess(Success);
        else onFailure(Failure);
    }

    public override bool Equals(object? obj) => Equals(obj as Result<TSuccess, TFailure>);
    public override int GetHashCode() => HashCode.Combine(State, Success, Failure);
}
