using System.Reflection;
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

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    // Static class to cache formatter instances
    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T> Formatter;

        static FormatterCache()
        {
            // Initialize formatters based on type
            if (typeof(T) == typeof(ExitCode))
            {
                Formatter = (IMessagePackFormatter<T>)new ExitCodeFormatter();
            }
            else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var formatterType = typeof(ResultFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType);
            }
            else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ResultWithPayload<>))
            {
                var formatterType =
                    typeof(ResultWithPayloadFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType);
            }
            else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Payload<>))
            {
                var formatterType = typeof(PayloadFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType);
            }
            else
            {
                // Check if T is a type for which a dynamic formatter can be applied
                if (CanBeDynamicallyFormatted(typeof(T)))
                    Formatter = CreateDynamicFormatter<T>();
                else
                    // Fallback to standard resolver if no custom or dynamic formatter is found
                    Formatter = StandardResolver.Instance.GetFormatter<T>();
            }
        }

        private static bool CanBeDynamicallyFormatted(Type type)
        {
            // Check for public instance properties
            var hasPublicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(prop => prop.CanRead && prop.GetGetMethod(true).IsPublic);

            // Check for public instance fields
            var hasPublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Any();

            return hasPublicProperties || hasPublicFields;
        }

        private static IMessagePackFormatter<T> CreateDynamicFormatter<T>()
        {
            return new DynamicFormatter<T>();
        }

        
    }
}