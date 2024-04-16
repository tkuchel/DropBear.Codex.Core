using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation that may return either a success value of type
///     <typeparamref name="TSuccess" />
///     or a failure value of type <typeparamref name="TFailure" />.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value.</typeparam>
/// <typeparam name="TFailure">The type of the failure value.</typeparam>
#pragma warning disable MA0048
public class Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
#pragma warning restore MA0048
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{TSuccess, TFailure}" /> class.
    /// </summary>
    /// <param name="success">The success value.</param>
    /// <param name="failure">The failure value.</param>
    /// <param name="state">The state of the result (Success or Failure).</param>
    internal Result(TSuccess success, TFailure failure, ResultState state)
    {
        Success = success;
        Failure = failure;
        State = state;
    }

    /// <summary>
    ///     Gets the success value.
    /// </summary>
    public TSuccess Success { get; }

    /// <summary>
    ///     Gets the failure value.
    /// </summary>
    public TFailure Failure { get; }

    /// <summary>
    ///     Gets the state of the result (Success or Failure).
    /// </summary>
    public ResultState State { get; }

    /// <inheritdoc />
    public bool Equals(Result<TSuccess, TFailure>? other)
    {
        if (other is null) return false;
        return State == other.State && EqualityComparer<TSuccess>.Default.Equals(Success, other.Success) &&
               EqualityComparer<TFailure>.Default.Equals(Failure, other.Failure);
    }

    /// <summary>
    ///     Implicitly converts a value of type <typeparamref name="TSuccess" /> to a success result.
    /// </summary>
    /// <param name="success">The success value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TSuccess success) =>
        ResultFactory.Success<TSuccess, TFailure>(success);

    /// <summary>
    ///     Implicitly converts a value of type <typeparamref name="TFailure" /> to a failure result.
    /// </summary>
    /// <param name="failure">The failure value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) =>
        ResultFactory.Failure<TSuccess, TFailure>(failure);

    /// <summary>
    ///     Matches the result with the specified functions, depending on whether it's a success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result to return.</typeparam>
    /// <param name="onSuccess">The function to apply if the operation was successful.</param>
    /// <param name="onFailure">The function to apply if the operation failed.</param>
    /// <returns>The result of applying the corresponding function based on the result state.</returns>
    [Pure]
    public T Match<T>(Func<TSuccess, T> onSuccess, Func<TFailure, T> onFailure) =>
        State switch
        {
            ResultState.Success => onSuccess(Success),
            ResultState.Failure => onFailure(Failure),
            _ => throw new InvalidOperationException("Invalid state")
        };

    /// <summary>
    ///     Executes the specified actions depending on whether the result is a success or failure.
    /// </summary>
    /// <param name="onSuccess">The action to execute if the operation was successful.</param>
    /// <param name="onFailure">The action to execute if the operation failed.</param>
    public void Match(Action<TSuccess> onSuccess, Action<TFailure> onFailure)
    {
        if (State is ResultState.Success) onSuccess(Success);
        else onFailure(Failure);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Result<TSuccess, TFailure>);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(State, Success, Failure);
}
