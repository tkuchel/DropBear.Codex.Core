#region

using System.Collections.ObjectModel;

#endregion

namespace DropBear.Codex.Core;

public class Result : IEquatable<Result>
{
    protected internal Result(ResultState state, string? error, Exception? exception)
    {
        if (state is ResultState.Failure or ResultState.PartialSuccess && string.IsNullOrEmpty(error))
        {
            throw new ArgumentException("An error message must be provided for non-success results.", nameof(error));
        }

        State = state;
        ErrorMessage = error ?? string.Empty;
        Exception = exception;
    }

    public ResultState State { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public ReadOnlyCollection<Exception> Exceptions { get; internal set; } = new(new List<Exception>());
    public bool IsSuccess => State is ResultState.Success or ResultState.PartialSuccess;

    public bool Equals(Result? other)
    {
        return other is not null && State == other.State && ErrorMessage == other.ErrorMessage &&
               Equals(Exception, other.Exception) && Exceptions.SequenceEqual(other.Exceptions);
    }

    public static Result Success()
    {
        return new Result(ResultState.Success, string.Empty, null);
    }

    public static Result Failure(string error, Exception? exception = null)
    {
        return new Result(ResultState.Failure, error, exception);
    }

    public static Result Failure(IEnumerable<Exception> exceptions)
    {
        var exceptionList = exceptions.ToList();
        var errorMessage = exceptionList.Count > 0 ? exceptionList[0].Message : "Multiple errors occurred.";
        return new Result(ResultState.Failure, errorMessage, exceptionList.FirstOrDefault())
        {
            Exceptions = new ReadOnlyCollection<Exception>(exceptionList)
        };
    }

    public static Result Warning(string error)
    {
        return new Result(ResultState.Warning, error, null);
    }

    public static Result PartialSuccess(string error)
    {
        return new Result(ResultState.PartialSuccess, error, null);
    }

    public static Result Cancelled(string error)
    {
        return new Result(ResultState.Cancelled, error, null);
    }

    public Result Unwrap()
    {
        return this ?? throw new InvalidOperationException("Cannot unwrap a result that is not a Result<Result>.");
    }

    public Result OnFailure(Action<string, Exception?> action)
    {
        if (State is ResultState.Failure)
        {
            SafeExecute(() => action(ErrorMessage, Exception));
        }

        return this;
    }

    public void OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            SafeExecute(action);
        }
    }

    public void OnWarning(Action<string> action)
    {
        if (State is ResultState.Warning)
        {
            SafeExecute(() => action(ErrorMessage));
        }
    }

    public Task OnSuccessAsync(Func<Task> action)
    {
        return IsSuccess ? SafeExecuteAsync(action) : Task.CompletedTask;
    }

    public Task OnFailureAsync(Func<string, Exception?, Task> action)
    {
        return State is ResultState.Failure
            ? SafeExecuteAsync(() => action(ErrorMessage, Exception))
            : Task.CompletedTask;
    }

    public Result Map(
        Func<Result> onSuccess,
        Func<string, Exception?, Result> onFailure,
        Func<string, Result>? onWarning = null,
        Func<string, Result>? onPartialSuccess = null,
        Func<string, Result>? onCancelled = null,
        Func<string, Result>? onPending = null,
        Func<string, Result>? onNoOp = null)
    {
        return Match(onSuccess, onFailure, onWarning, onPartialSuccess, onCancelled, onPending, onNoOp);
    }

    public T Match<T>(
        Func<T> onSuccess,
        Func<string, Exception?, T> onFailure,
        Func<string, T>? onWarning = null,
        Func<string, T>? onPartialSuccess = null,
        Func<string, T>? onCancelled = null,
        Func<string, T>? onPending = null,
        Func<string, T>? onNoOp = null)
    {
        return State switch
        {
            ResultState.Success => onSuccess(),
            ResultState.Failure => onFailure(ErrorMessage, Exception),
            ResultState.Warning => InvokeOrDefault(onWarning, onFailure),
            ResultState.PartialSuccess => InvokeOrDefault(onPartialSuccess, onFailure),
            ResultState.Cancelled => InvokeOrDefault(onCancelled, onFailure),
            ResultState.Pending => InvokeOrDefault(onPending, onFailure),
            ResultState.NoOp => InvokeOrDefault(onNoOp, onFailure),
            _ => throw new InvalidOperationException("Unhandled result state.")
        };

        T InvokeOrDefault(Func<string, T>? handler, Func<string, Exception?, T> defaultHandler)
        {
            return handler != null ? handler(ErrorMessage) : defaultHandler(ErrorMessage, Exception);
        }
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
            Console.WriteLine($"Exception during action execution: {ex.Message}");
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
            Console.WriteLine($"Exception during asynchronous action execution: {ex.Message}");
        }
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Result);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(State, ErrorMessage, Exception);
    }
}
