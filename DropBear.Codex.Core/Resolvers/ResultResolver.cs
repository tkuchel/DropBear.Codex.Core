using DropBear.Codex.Core.Formatters;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Resolvers;

public class ResultResolver : IFormatterResolver
{
    public static readonly ResultResolver Instance = new ResultResolver();

    private readonly Dictionary<Type, object> formatters = new Dictionary<Type, object>();

    private ResultResolver()
    {
        // Register the custom formatter for Result
        formatters.Add(typeof(Result), new ResultFormatter());
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (formatters.TryGetValue(typeof(T), out var formatter))
        {
            return formatter as IMessagePackFormatter<T>;
        }

        // Return null if no formatter is found for the type T
        return null;
    }
}