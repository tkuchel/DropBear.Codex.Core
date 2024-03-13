using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Helpers;
using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes;

/// <summary>
///     Represents the result of an operation, encapsulating success with a value of type T1 or failure with a value of
///     type T2.
/// </summary>
/// <typeparam name="T1">The type of the success value.</typeparam>
/// <typeparam name="T2">The type of the failure value.</typeparam>
[MessagePackObject]
#pragma warning disable MA0048
public class Result<T1, T2>
#pragma warning restore MA0048
{
    private readonly T2? _failureValue;
    private readonly T1? _successValue;

    /// <summary>
    ///     For deserialization purposes only. Not intended for direct use in code.
    /// </summary>
    [Obsolete("For deserialization purposes only. Not intended for direct use in code.")]
    public Result()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the Result class with a success value.
    /// </summary>
    /// <param name="successValue">The success value of type T1.</param>
    public Result(T1 successValue)
    {
        _successValue = successValue;
        ExitCode = StandardExitCodes.Success;
    }

    /// <summary>
    ///     Initializes a new instance of the Result class with a failure value.
    /// </summary>
    /// <param name="failureValue">The failure value of type T2.</param>
    /// <param name="exitCode">The exit code for the failure</param>
    public Result(T2 failureValue, ExitCode? exitCode = null)
    {
        _failureValue = failureValue;
        ExitCode = exitCode == null ? StandardExitCodes.UnspecifiedError : exitCode;
    }


    /// <summary>
    ///     Gets the exit code representing the outcome of the operation.
    /// </summary>
    [Key(0)]
    public ExitCode? ExitCode { get; }

    /// <summary>
    ///     Indicates whether the operation was successful.
    /// </summary>
    [IgnoreMember]
    public bool IsSuccess => ExitCode == StandardExitCodes.Success;

    /// <summary>
    ///     Indicates whether the operation failed.
    /// </summary>
    [IgnoreMember]
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsFailure => !IsSuccess;

    [IgnoreMember] private bool HasSuccessValue => IsSuccess && _successValue is not null;

    // Indicates if the failure value is available
    [IgnoreMember] private bool HasFailureValue => IsFailure && _failureValue is not null;

    /// <summary>
    ///     Gets the success value. Throws InvalidOperationException if the result is a failure.
    /// </summary>
    [Key(1)]
    public T1? SuccessValue => HasSuccessValue ? _successValue : default;

    /// <summary>
    ///     Gets the failure value. Throws InvalidOperationException if the result is a success.
    /// </summary>
    [Key(2)]
    public T2? FailureValue => HasFailureValue ? _failureValue : default;

    /// <summary>
    ///     Executes a specified action if the result is a success.
    /// </summary>
    /// <param name="successAction">The action to execute if the result is successful.</param>
    /// <returns>The current Result instance for fluent chaining.</returns>
    public Result<T1, T2> OnSuccess(Action<T1> successAction)
    {
        if (!IsSuccess) return this;
        if (_successValue is not null)
            successAction(_successValue);
        return this;
    }

    /// <summary>
    ///     Executes a specified action if the result is a failure.
    /// </summary>
    /// <param name="failureAction">The action to execute if the result is a failure.</param>
    /// <returns>The current Result instance for fluent chaining.</returns>
    public Result<T1, T2> OnFailure(Action<T2> failureAction)
    {
        if (!IsFailure) return this;
        if (_failureValue is not null)
            failureAction(_failureValue);
        return this;
    }

    /// <summary>
    ///     Executes one of two functions based on the result state, returning a value of type TResult.
    /// </summary>
    /// <param name="successFunc">The function to execute if the result is successful.</param>
    /// <param name="failureFunc">The function to execute if the result is a failure.</param>
    /// <returns>A value of type TResult derived from the executed function.</returns>
    public TResult? Match<TResult>(Func<T1, TResult> successFunc, Func<T2, TResult> failureFunc)
    {
        if (_successValue is null) return default;
        if (_failureValue is not null)
            return IsSuccess ? successFunc(_successValue) : failureFunc(_failureValue);
        return default;
    }

    /// <summary>
    ///     Asynchronously executes a specified function if the result is a success.
    /// </summary>
    /// <param name="successFunc">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A Task representing the asynchronous operation and current Result instance for fluent chaining.</returns>
    public async Task<Result<T1, T2>> OnSuccessAsync(Func<T1, Task> successFunc)
    {
        if (!IsSuccess) return this;
        if (_successValue is not null)
            await successFunc(_successValue).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    ///     Asynchronously executes a specified function if the result is a failure.
    /// </summary>
    /// <param name="failureFunc">The asynchronous function to execute if the result is a failure.</param>
    /// <returns>A Task representing the asynchronous operation and current Result instance for fluent chaining.</returns>
    public async Task<Result<T1, T2>> OnFailureAsync(Func<T2, Task> failureFunc)
    {
        if (!IsFailure) return this;
        if (_failureValue is not null)
            await failureFunc(_failureValue).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    ///     Asynchronously executes one of two functions based on the result state, returning a value of type TResult.
    /// </summary>
    /// <param name="successFunc">The asynchronous function to execute if the result is successful.</param>
    /// <param name="failureFunc">The asynchronous function to execute if the result is a failure.</param>
    /// <returns>
    ///     A Task representing the asynchronous operation with a value of type TResult derived from the executed
    ///     function.
    /// </returns>
    public async Task<TResult?> MatchAsync<TResult>(Func<T1, Task<TResult>> successFunc,
        Func<T2, Task<TResult>> failureFunc)
    {
        if (_successValue is null) return default;
        if (_failureValue is not null)
            return IsSuccess
                ? await successFunc(_successValue).ConfigureAwait(false)
                : await failureFunc(_failureValue).ConfigureAwait(false);
        return default;
    }

    // Implicit operator for converting to Result<T1, T2> from T1
    public static implicit operator Result<T1, T2>(T1 successValue) => new(successValue);

    // Implicit operator for converting to Result<T1, T2> from T2
    public static implicit operator Result<T1, T2>(T2 failureValue) =>
        new(failureValue, StandardExitCodes.UnspecifiedError); // Modify the exit code as needed

    // Optional: Implicit operators for converting from Result<T1, T2> to T1 or T2.
    public static implicit operator T1?(Result<T1?, T2> result) => result.SuccessValue ?? default;

    public static implicit operator T2?(Result<T1, T2?> result) => result.FailureValue ?? default;
}
