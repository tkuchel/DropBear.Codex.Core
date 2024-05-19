using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation that may return either a success value of type TSuccess
///     or a failure value of type TFailure.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value.</typeparam>
/// <typeparam name="TFailure">The type of the failure value.</typeparam>
#pragma warning disable MA0048
public class Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
#pragma warning restore MA0048
{
    /// <summary>
    ///     Initializes a new instance of the Result class.
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

    public TSuccess Success { get; }
    public TFailure Failure { get; }
    public ResultState State { get; }

    public bool Equals(Result<TSuccess, TFailure>? other) =>
        other is not null && State == other.State &&
        EqualityComparer<TSuccess>.Default.Equals(Success, other.Success) &&
        EqualityComparer<TFailure>.Default.Equals(Failure, other.Failure);

    /// <summary>
    ///     Implicitly converts a value of type TSuccess to a success result.
    /// </summary>
    /// <param name="success">The success value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TSuccess success) =>
        new(success, default!, ResultState.Success);

    /// <summary>
    ///     Implicitly converts a value of type TFailure to a failure result.
    /// </summary>
    /// <param name="failure">The failure value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) =>
        new(default!, failure, ResultState.Failure);

    /// <summary>
    ///     Matches the result with the specified functions based on the current state.
    /// </summary>
    /// <typeparam name="T">The type of the result to return.</typeparam>
    /// <param name="onSuccess">
    ///     The function to apply if the operation was successful. Takes the success value and returns a
    ///     result of type T.
    /// </param>
    /// <param name="onFailure">
    ///     The function to apply if the operation failed. Takes the failure value and returns a result of
    ///     type T.
    /// </param>
    /// <param name="onPending">The function to invoke if the operation is pending. Returns a result of type T.</param>
    /// <param name="onCancelled">The function to invoke if the operation was cancelled. Returns a result of type T.</param>
    /// <param name="onWarning">The function to invoke if the operation resulted in a warning. Returns a result of type T.</param>
    /// <param name="onPartialSuccess">
    ///     The function to invoke if the operation achieved partial success. Returns a result of
    ///     type T.
    /// </param>
    /// <param name="onNoOp">The function to invoke if no operation was performed. Returns a result of type T.</param>
    /// <returns>The result of applying the corresponding function based on the result state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unhandled result state is encountered.</exception>
    [Pure]
    public T? Match<T>(
        Func<TSuccess, T> onSuccess,
        Func<TFailure, T> onFailure,
        Func<T>? onPending = null,
        Func<T>? onCancelled = null,
        Func<T>? onWarning = null,
        Func<T>? onPartialSuccess = null,
        Func<T>? onNoOp = null)
    {
        // Use provided functions or default no-op functions
        return State switch
        {
            ResultState.Success => onSuccess(Success),
            ResultState.Failure => onFailure(Failure),
            ResultState.Pending => onPending is not null ? onPending() : DefaultFunc(),
            ResultState.Cancelled => onCancelled is not null ? onCancelled() : DefaultFunc(),
            ResultState.Warning => onWarning is not null ? onWarning() : DefaultFunc(),
            ResultState.PartialSuccess => onPartialSuccess is not null ? onPartialSuccess() : DefaultFunc(),
            ResultState.NoOp => onNoOp is not null ? onNoOp() : DefaultFunc(),
            _ => throw new InvalidOperationException("Unhandled state")
        };

        // Provide a default no-op function that returns a default value of type T
        T? DefaultFunc()
        {
            return default;
        }
    }


    /// <summary>
    ///     Executes the specified actions based on the current state of the result.
    /// </summary>
    /// <param name="onSuccess">The action to execute if the operation was successful. Takes the success value.</param>
    /// <param name="onFailure">The action to execute if the operation failed. Takes the failure value.</param>
    /// <param name="onPending">The action to execute if the operation is pending.</param>
    /// <param name="onCancelled">The action to execute if the operation was cancelled.</param>
    /// <param name="onWarning">The action to execute if the operation resulted in a warning.</param>
    /// <param name="onPartialSuccess">The action to execute if the operation achieved partial success.</param>
    /// <param name="onNoOp">The action to execute if no operation was performed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unhandled result state is encountered.</exception>
    public void Match(
        Action<TSuccess> onSuccess,
        Action<TFailure> onFailure,
        Action? onPending = null,
        Action? onCancelled = null,
        Action? onWarning = null,
        Action? onPartialSuccess = null,
        Action? onNoOp = null)
    {
        // Default no-op action does nothing
        var defaultAction = () => { };

        switch (State)
        {
            case ResultState.Success:
                onSuccess(Success);
                break;
            case ResultState.Failure:
                onFailure(Failure);
                break;
            case ResultState.Pending:
                (onPending ?? defaultAction)();
                break;
            case ResultState.Cancelled:
                (onCancelled ?? defaultAction)();
                break;
            case ResultState.Warning:
                (onWarning ?? defaultAction)();
                break;
            case ResultState.PartialSuccess:
                (onPartialSuccess ?? defaultAction)();
                break;
            case ResultState.NoOp:
                (onNoOp ?? defaultAction)();
                break;
            default:
#pragma warning disable CA2208
#pragma warning disable MA0015
                throw new ArgumentOutOfRangeException(nameof(State), "Unhandled result state encountered.");
#pragma warning restore MA0015
#pragma warning restore CA2208
        }
    }

// In the Result<TSuccess, TFailure> class
    public Result<TNewSuccess, TFailure> Bind<TNewSuccess>(Func<TSuccess, Result<TNewSuccess, TFailure>> onSuccess) =>
        Match(
            success => onSuccess(success),
            failure => new Result<TNewSuccess, TFailure>(default!, failure, ResultState.Failure)
        ) ?? throw new InvalidOperationException("Match function returned null.");

    public override bool Equals(object? obj) => Equals(obj as Result<TSuccess, TFailure>);
    public override int GetHashCode() => HashCode.Combine(State, Success, Failure);
}
