using DropBear.Codex.Core.Resolvers;
using MessagePack;

namespace DropBear.Codex.Core.Helpers;

public static class SerializationHelper
{
    private static readonly HashSet<Type> RegisteredTypes = new HashSet<Type>();
    private static readonly object Lock = new object();

    public static byte[] Serialize<T>(T obj)
    {
        var type = typeof(T);
        EnsureFormatterRegistered(type);
        
        var options = MessagePackSerializerOptions.Standard.WithResolver(DynamicCompositeResolver.GetResolver());
        return MessagePackSerializer.Serialize(obj, options);
    }

    private static void EnsureFormatterRegistered(Type type)
    {
        lock (Lock)
        {
            if (!RegisteredTypes.Contains(type))
            {
                var resolver = CreateResolverForType(type);
                DynamicCompositeResolver.RegisterResolver(resolver);
                RegisteredTypes.Add(type);
            }
        }
    }

    private static IFormatterResolver CreateResolverForType(Type type)
    {
        // Dynamically create a resolver or formatter for the type
        // This method needs to be implemented based on your application's needs
        return null; // Placeholder
    }
}