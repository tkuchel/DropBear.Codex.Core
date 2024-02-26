using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class DynamicFormatter<T> : IMessagePackFormatter<T>
{
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        // Begin map or array format
        // Reflect over T to find properties and fields to serialize
        // Use options.Resolver.GetFormatterWithVerify for each property/field type to serialize them
    }

    public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Implement dynamic deserialization logic
        // Use reflection to find properties and fields to deserialize
        // Use options.Resolver.GetFormatterWithVerify for each property/field type to deserialize them
        // Return the constructed object of type T
    }
}