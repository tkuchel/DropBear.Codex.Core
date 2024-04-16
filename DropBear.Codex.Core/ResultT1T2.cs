namespace DropBear.Codex.Core;

#pragma warning disable MA0048
public class Result<TSuccess, TFailure>
#pragma warning restore MA0048
{
    public enum ResultState
    {
        Success,
        Failure
    }

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

    public static implicit operator Result<TSuccess, TFailure>(TSuccess success) =>
        ResultFactory.Success<TSuccess, TFailure>(success);

    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) =>
        ResultFactory.Failure<TSuccess, TFailure>(failure);


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
}
