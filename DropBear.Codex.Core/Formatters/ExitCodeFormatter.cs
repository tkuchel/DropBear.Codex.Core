using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;
using MessagePack.Formatters;

// Updated custom formatter for ExitCode compatible with MessagePack V2
namespace DropBear.Codex.Core.Formatters;

public class ExitCodeFormatter<T> : IMessagePackFormatter<T> where T : ExitCode
{
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        // Start an array container to hold the properties
        writer.WriteArrayHeader(2);
        writer.Write(value.Code); // Writes an int32 without needing to explicitly call WriteInt32

        // Convert string to UTF-8 bytes and write
        writer.Write(value.Description); // Directly write the string which internally converts to UTF-8 bytes
    }

    public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        // Ensure we're reading an array of the expected length
        var length = reader.ReadArrayHeader();
        if (length != 2)
        {
            throw new MessagePackSerializationException("Expected array length of 2.");
        }

        var code = reader.ReadInt32();
        var description = reader.ReadString();

        reader.Depth--;

        return (T)Activator.CreateInstance(typeof(T), code, description);
    }
}