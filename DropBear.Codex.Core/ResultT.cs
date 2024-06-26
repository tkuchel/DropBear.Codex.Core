﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

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
    public ReadOnlyCollection<Exception> Exceptions { get; private set; } = new(new List<Exception>());

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => State is ResultState.Success or ResultState.PartialSuccess;

    public bool Equals(Result<T>? other) =>
        other is not null && State == other.State && EqualityComparer<T>.Default.Equals(Value, other.Value) &&
        ErrorMessage == other.ErrorMessage && Equals(Exception, other.Exception) &&
        Exceptions.SequenceEqual(other.Exceptions);

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
    ///     Creates a failure result with the specified collection of exceptions.
    /// </summary>
    /// <param name="exceptions">The collection of exceptions describing the failure.</param>
    /// <returns>A failure result with the specified collection of exceptions.</returns>
    [Pure]
#pragma warning disable CA1000
    public static Result<T> Failure(Collection<Exception> exceptions)
    {
        var errorMessage = exceptions.Count > 0 ? exceptions.First().Message : "Multiple errors occurred.";
        return new Result<T>(default!, errorMessage, exceptions.FirstOrDefault(), ResultState.Failure)
        {
            Exceptions = new ReadOnlyCollection<Exception>(exceptions.ToList())
        };
    }
#pragma warning restore CA1000

    /// <summary>
    ///     Binds the result to another operation.
    /// </summary>
    /// <param name="func">The function to apply to the current result value if the operation was successful.</param>
    /// <returns>The result of applying the function to the current result value.</returns>
    [Pure]
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func) =>
        State is ResultState.Success
            ? func(Value)
            : Result<TOut>.Failure(ErrorMessage ?? "An unknown error has occurred", Exception);

    /// <summary>
    ///     Executes the specified action if the operation was successful, passing the result value.
    ///     If the operation failed, a failure result with the error message is returned.
    /// </summary>
    /// <param name="action">The action to execute if the operation was successful.</param>
    /// <returns>The original result if the operation was successful, or a failure result with the error message.</returns>
    [Pure]
    public Result OnSuccess(Func<T, Result> action) =>
        State switch
        {
            ResultState.Success => SafeExecute(() => action(Value)),
            _ => Result.Failure(ErrorMessage ?? "An unknown error has occurred.")
        };

    /// <summary>
    ///     Executes the specified function if the operation was successful, passing the result value.
    ///     If the operation failed, a failure result with the error message is returned.
    /// </summary>
    /// <typeparam name="TOut">The type of the returned result value.</typeparam>
    /// <param name="func">The function to execute if the operation was successful.</param>
    /// <returns>
    ///     The result of executing the function if the operation was successful, or a failure result with the error
    ///     message.
    /// </returns>
    [Pure]
    public Result<TOut> OnSuccess<TOut>(Func<T, Result<TOut>> func) =>
        State switch
        {
            ResultState.Success => SafeExecute(() => func(Value)),
            _ => Result<TOut>.Failure(ErrorMessage ?? "An unknown error has occurred.")
        };

    /// <summary>
    ///     Executes the specified action if the operation failed.
    /// </summary>
    /// <param name="action">The action to execute if the operation failed.</param>
    /// <returns>The current result instance.</returns>
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (State is not ResultState.Failure) return this;
        try
        {
            action(ErrorMessage ?? string.Empty, Exception);
        }
        catch (Exception ex)
        {
            // Handle exception, log it, or propagate as needed
            Console.WriteLine("Error executing action: " + ex.Message);
        }

        return this;
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, Exception?, TResult> onFailure,
        Func<string, TResult>? onWarning = null, Func<string, TResult>? onPartialSuccess = null,
        Func<string, TResult>? onCancelled = null, Func<string, TResult>? onPending = null,
        Func<string, TResult>? onNoOp = null)
    {
        return State switch
        {
            ResultState.Success => onSuccess(Value),
            ResultState.Failure => onFailure(ErrorMessage ?? "An unknown error has occurred.", Exception),
            ResultState.Warning => InvokeHandler(onWarning),
            ResultState.PartialSuccess => InvokeHandler(onPartialSuccess),
            ResultState.Cancelled => InvokeHandler(onCancelled),
            ResultState.Pending => InvokeHandler(onPending),
            ResultState.NoOp => InvokeHandler(onNoOp),
            _ => throw new InvalidOperationException("Unhandled result state.")
        };

        TResult InvokeHandler(Func<string, TResult>? handler)
        {
            return handler is not null
                ? handler(ErrorMessage ?? "An unknown error has occurred.")
                : onFailure(ErrorMessage ?? "An unknown error has occurred.", Exception);
        }
    }

    /// <summary>
    ///     Unwraps the error message from the result.
    ///     If the operation was successful, the specified default error message is returned.
    /// </summary>
    /// <param name="defaultError">The default error message to return if the operation was successful.</param>
    /// <returns>
    ///     The error message if the operation failed, or the specified default error message if the operation was
    ///     successful.
    /// </returns>
    [Pure]
    public string UnwrapError(string defaultError = "") =>
        State switch
        {
            ResultState.Success => defaultError,
            _ => ErrorMessage ?? "An unknown error has occurred."
        };

    /// <summary>
    ///     Unwraps the result when it is a Result&lt;T&gt;>.
    /// </summary>
    /// <returns>The unwrapped result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is not a Result&lt;T&gt;.</exception>
    public Result<T> Unwrap()
    {
        if (this is { } result)
            return result.Value;

        throw new InvalidOperationException("Cannot unwrap a result that is not a Result<T>.");
    }

    /// <summary>
    ///     Maps the current result to a new result with a different value type using the provided mapping function.
    /// </summary>
    /// <typeparam name="TOut">The type of the new result value.</typeparam>
    /// <param name="mapper">The function to map the current result value to a new value.</param>
    /// <returns>
    ///     A new result with the mapped value if the current result is successful, or a failure result with the same
    ///     error information.
    /// </returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        State switch
        {
            ResultState.Success => Result<TOut>.Success(mapper(Value)),
            _ => Result<TOut>.Failure(ErrorMessage ?? "An unknown error has occurred.", Exception)
        };

    private static Result SafeExecute(Func<Result> action)
    {
        try { return action(); }
        catch (Exception ex)
        {
            // Handle exception, log it, or propagate as needed
            Console.WriteLine("Error executing action: " + ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private static Result<TOut> SafeExecute<TOut>(Func<Result<TOut>> func)
    {
        try { return func(); }
        catch (Exception ex)
        {
            // Handle exception, log it, or propagate as needed
            Console.WriteLine("Error executing function: " + ex.Message);
            return Result<TOut>.Failure(ex.Message);
        }
    }

    public override bool Equals(object? obj) => Equals(obj as Result<T>);

    public override int GetHashCode() => HashCode.Combine(State, Value, ErrorMessage, Exception);

    // Implicit conversion from T to Result<T> (Success)
    public static implicit operator Result<T>(T value) => Success(value);

    // Implicit conversion from Exception to Result<T> (Failure)
    public static implicit operator Result<T>(Exception exception) => Failure(exception);
}
