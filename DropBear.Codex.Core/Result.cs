namespace DropBear.Codex.Core;

/// <summary>
///     Represents the result of an operation, indicating success or failure along with optional error information.
/// </summary>
public class Result : IEquatable<Result>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Result" /> class.
    /// </summary>
    /// <param name="state">Indicates the result state of the operation.</param>
    /// <param name="error">The error message if the operation failed.</param>
    /// <param name="exception">The exception that occurred during the operation, if any.</param>
    protected internal Result(ResultState state, string? error, Exception? exception)
    {
        if (state is ResultState.Failure or ResultState.PartialSuccess && string.IsNullOrEmpty(error))
            throw new ArgumentException("An error message must be provided for non-success results.", nameof(error));

        State = state;
        Error = error ?? string.Empty;
        Exception = exception;
    }

    public ResultState State { get; }
    public string Error { get; }
    public Exception? Exception { get; }
    public bool IsSuccess => State is ResultState.Success or ResultState.PartialSuccess;

    public bool Equals(Result? other) =>
        other is not null && State == other.State && Error == other.Error && Equals(Exception, other.Exception);

    /// <summary>
    ///     Creates a result indicating a successful operation.
    /// </summary>
    /// <returns>A success result.</returns>
    public static Result Success() => new(ResultState.Success, string.Empty, null);

    /// <summary>
    ///     Creates a result indicating a failed operation with a specific error message and optional exception.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if available.</param>
    /// <returns>A failure result.</returns>
    public static Result Failure(string error, Exception? exception = null) =>
        new(ResultState.Failure, error, exception);

    /// <summary>
    ///     Creates a result indicating an operation resulted in a warning.
    /// </summary>
    /// <param name="error">The warning message.</param>
    /// <returns>A warning result.</returns>
    public static Result Warning(string error) =>
        new(ResultState.Warning, error, null);

    /// <summary>
    ///     Creates a result indicating an operation achieved partial success.
    /// </summary>
    /// <param name="error">The error message if partial issues occurred.</param>
    /// <returns>A partial success result.</returns>
    public static Result PartialSuccess(string error) =>
        new(ResultState.PartialSuccess, error, null);

    /// <summary>
    ///     Creates a result indicating an operation was cancelled.
    /// </summary>
    /// <param name="error">The error message describing the cancellation reason.</param>
    /// <returns>A cancelled result.</returns>
    public static Result Cancelled(string error) =>
        new(ResultState.Cancelled, error, null);

    public void OnSuccess(Action action)
    {
        if (IsSuccess)
            SafeExecute(action);
    }

    public void OnFailure(Action<string, Exception?> action)
    {
        if (State is ResultState.Failure)
            SafeExecute(() => action(Error, Exception));
    }

    public void OnWarning(Action<string> action)
    {
        if (State is ResultState.Warning)
            SafeExecute(() => action(Error));
    }

    public async Task OnSuccessAsync(Func<Task> action)
    {
        if (IsSuccess)
            await SafeExecuteAsync(action).ConfigureAwait(false);
    }

    public async Task OnFailureAsync(Func<string, Exception?, Task> action)
    {
        if (State is ResultState.Failure)
            await SafeExecuteAsync(() => action(Error, Exception)).ConfigureAwait(false);
    }

    private static void SafeExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            // Log exception or handle it as needed
            Console.WriteLine("Exception during action execution: " + ex.Message);
        }
    }

    private static async Task SafeExecuteAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log exception or handle it as needed
            Console.WriteLine("Exception during asynchronous action execution: " + ex.Message);
        }
    }

    public override bool Equals(object? obj) => Equals(obj as Result);
    public override int GetHashCode() => HashCode.Combine(State, Error, Exception);
}
