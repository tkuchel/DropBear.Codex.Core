namespace DropBear.Codex.Core;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T? value) where T : class
    {
        return value is not null ? Result<T>.Success(value) : Result<T>.Failure("Value is null");
    }

    public static Result<T> ToResult<T>(this T? value) where T : struct
    {
        return value.HasValue ? Result<T>.Success(value.Value) : Result<T>.Failure("Value is null");
    }

    public static Result<IEnumerable<T>> Traverse<T>(this IEnumerable<Result<T>> results)
    {
        var successes = new List<T>();
        var errors = new List<string>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                successes.Add(result.Value);
            }
            else
            {
                errors.Add(result.ErrorMessage);
            }
        }

        return errors.Count == 0
            ? Result<IEnumerable<T>>.Success(successes)
            : Result<IEnumerable<T>>.Failure(string.Join(", ", errors));
    }

    public static Result<IEnumerable<T>> Sequence<T>(this IEnumerable<Result<T>> results)
    {
        return results.Traverse();
    }
}
