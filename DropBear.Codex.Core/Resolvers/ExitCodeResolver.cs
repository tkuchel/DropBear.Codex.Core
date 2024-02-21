using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Formatters;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Resolvers;

// Custom resolver for ExitCode compatible with MessagePack V2
public class ExitCodeResolver : IFormatterResolver
{
    // Singleton instance for easy access
    public static readonly ExitCodeResolver Instance = new ExitCodeResolver();

    // Cache of formatters by type
    private readonly Dictionary<Type, object> formatters = new Dictionary<Type, object>();

    private ExitCodeResolver()
    {
        // Register the custom formatter for ExitCode and its derived classes
        formatters.Add(typeof(ExitCode), new ExitCodeFormatter<ExitCode>());
        // If there are specific formatters for derived classes, register them here
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (formatters.TryGetValue(typeof(T), out var formatter))
        {
            return formatter as IMessagePackFormatter<T>;
        }

        // Return null if no formatter is found for the type T
        // MessagePack will fallback to its default resolvers
        return null;
    }
}
