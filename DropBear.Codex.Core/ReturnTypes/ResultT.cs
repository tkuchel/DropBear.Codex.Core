using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Helpers;
using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes;

/// <summary>
///     Represents the result of an operation, encapsulating both success and failure states, along with an optional return
///     value for successful operations.
/// </summary>
/// <typeparam name="T">The type of the value returned in case of a successful operation.</typeparam>
[MessagePackObject]
public class Result<T> where T : notnull
{
    [Key(2)]
    private readonly T? _value;

    /// <summary>
    ///     For deserialization purposes only. Not intended for direct use in code.
    /// </summary>
    [Obsolete("For deserialization purposes only. Not intended for direct use in code.")]
    public Result()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the Result class with specified exit code, value, and error message.
    /// </summary>
    /// <param name="exitCode">The exit code representing the outcome of the operation.</param>
    /// <param name="value">The value associated with a successful result.</param>
    /// <param name="errorMessage">The error message associated with a failure.</param>
    private Result(ExitCode exitCode, T? value = default, string errorMessage = "")
    {
        ExitCode = exitCode ?? throw new ArgumentNullException(nameof(exitCode), "Exit code cannot be null.");
        ErrorMessage = errorMessage ?? string.Empty;
        _value = ExitCode == StandardExitCodes.Success ? value : default;
    }

    /// <summary>
    ///     Gets the exit code representing the outcome of the operation.
    /// </summary>
    [Key(0)]
    public ExitCode ExitCode { get; }

    /// <summary>
    ///     Gets the value associated with a successful result. Throws InvalidOperationException if the result is a failure.
    /// </summary>
    [Key(2)]
    public T Value => _value ?? throw new InvalidOperationException("Cannot access Value on a failed result.");

    /// <summary>
    ///     Indicates whether the operation was successful.
    /// </summary>
    [IgnoreMember]
    public bool IsSuccess => ExitCode == StandardExitCodes.Success;

    /// <summary>
    ///     Indicates whether the operation failed.
    /// </summary>
    [IgnoreMember]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the error message associated with a failure.
    /// </summary>
    [Key(1)]
    public string ErrorMessage { get; }

    /// <summary>
    ///     Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The value to be associated with the successful result.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(StandardExitCodes.Success, value);
    }

    /// <summary>
    ///     Creates a failure result with the specified error message and exit code.
    /// </summary>
    /// <param name="errorMessage">The error message for the failure.</param>
    /// <param name="exitCode">The exit code representing the specific type of failure.</param>
    /// <returns>A failure result.</returns>
    public static Result<T> Failure(string errorMessage, ExitCode exitCode = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(errorMessage));

        return new Result<T>(exitCode ?? StandardExitCodes.GeneralError, default, errorMessage);
    }

    /// <summary>
    ///     Executes the specified action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute on the result value.</param>
    public void OnSuccess(Action<T> action)
    {
        if (IsSuccess && _value != null)
            action(_value);
    }

    /// <summary>
    ///     Executes the specified action if the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute using the error message.</param>
    public void OnFailure(Action<string> action)
    {
        if (IsFailure) action(ErrorMessage);
    }

    /// <summary>
    ///     Matches the result to execute corresponding functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value of the functions.</typeparam>
    /// <param name="onSuccess">The function to execute on success, receiving the result value.</param>
    /// <param name="onFailure">The function to execute on failure, receiving the error message.</param>
    /// <returns>The result of executing either the onSuccess or onFailure function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value) : onFailure(ErrorMessage);
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the result is successful.
    /// </summary>
    /// <param name="action">The asynchronous action to execute on the result value.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task OnSuccessAsync(Func<T, Task> action)
    {
        if (IsSuccess && _value != null) await action(_value);
    }

    /// <summary>
    ///     Asynchronously executes the specified action if the result is a failure.
    /// </summary>
    /// <param name="action">The asynchronous action to execute using the error message.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task OnFailureAsync(Func<string, Task> action)
    {
        if (IsFailure) await action(ErrorMessage);
    }

    /// <summary>
    ///     Asynchronously matches the result to execute corresponding functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value of the functions, must not be null.</typeparam>
    /// <param name="onSuccess">The asynchronous function to execute on success, receiving the result value.</param>
    /// <param name="onFailure">The asynchronous function to execute on failure, receiving the error message.</param>
    /// <returns>
    ///     A Task representing the asynchronous operation, containing the result of executing either the onSuccess or
    ///     onFailure function.
    /// </returns>
    public async Task<Result<TResult>> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess,
        Func<string, Task<Result<TResult>>> onFailure) where TResult : notnull
    {
        return IsSuccess ? Result<TResult>.Success(await onSuccess(_value)) : await onFailure(ErrorMessage);
    }
}