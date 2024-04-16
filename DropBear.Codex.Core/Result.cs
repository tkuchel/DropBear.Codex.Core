using System.Diagnostics.Contracts;

namespace DropBear.Codex.Core;

public class Result : IEquatable<Result>
{
    protected Result(bool isSuccess, string error, Exception? exception)
    {
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new ArgumentException("An error message must be provided for failure results.", nameof(error));

        IsSuccess = isSuccess;
        Error = error ?? string.Empty; // Ensure error is never null
        Exception = exception;
    }

    public bool IsSuccess { get; }
    public string Error { get; }
    public Exception? Exception { get; }

    [Pure]
    public static Result Success() => new(isSuccess: true, string.Empty, exception: null);

    [Pure]
    public static Result Failure(string error, Exception? exception = null)
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentException("Error message cannot be null or empty.", nameof(error));

        return new Result(false, error, exception);
    }

    public void OnSuccess(Action action)
    {
        if (IsSuccess) action();
    }

    public void OnFailure(Action<string, Exception?> action)
    {
        if (!IsSuccess) action(Error, Exception);
    }

    public void OnBoth(Action<Result> action) => action(this);

    public async Task OnSuccessAsync(Func<Task> action)
    {
        if (IsSuccess) await action().ConfigureAwait(false);
    }

    public async Task OnFailureAsync(Func<string, Exception?, Task> action)
    {
        if (!IsSuccess) await action(Error, Exception).ConfigureAwait(false);
    }

    public async Task OnBothAsync(Func<Result, Task> action) => await action(this).ConfigureAwait(false);
    
    public bool Equals(Result? other)
    {
        if (other is null) return false;
        return IsSuccess == other.IsSuccess && Error == other.Error && Equals(Exception, other.Exception);
    }

    public override bool Equals(object? obj) => Equals(obj as Result);
    public override int GetHashCode() => HashCode.Combine(IsSuccess, Error, Exception);

}
