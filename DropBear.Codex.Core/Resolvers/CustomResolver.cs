using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Formatters;
using DropBear.Codex.Core.Models;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace DropBear.Codex.Core.Resolvers;

public class CustomResolver : IFormatterResolver
{
    // Singleton instance of the resolver
    public static readonly CustomResolver Instance = new();

    private CustomResolver()
    {
    }

    public IMessagePackFormatter<T>? GetFormatter<T>() => FormatterCache<T>.Formatter;

    // Static class to cache formatter instances
    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter = CreateFormatter();

        private static IMessagePackFormatter<T>? CreateFormatter()
        {
            // Initialize formatters based on type
            if (typeof(T) == typeof(ExitCode)) return (IMessagePackFormatter<T>)new ExitCodeFormatter();

            if (!typeof(T).IsGenericType) return StandardResolver.Instance.GetFormatter<T>();
            if (typeof(T).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var formatterType = typeof(ResultFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
            }

            if (typeof(T).GetGenericTypeDefinition() == typeof(ResultWithPayload<>))
            {
                var formatterType =
                    typeof(ResultWithPayloadFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
            }

            if (typeof(T).GetGenericTypeDefinition() != typeof(Payload<>))
                return StandardResolver.Instance.GetFormatter<T>();
            {
                var formatterType = typeof(PayloadFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
            }

            // Fallback to standard resolver if no custom or dynamic formatter is found
        }
    }
}
