using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ExitCodes.Standard;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class ExitCodeFormatter : IMessagePackFormatter<ExitCode>
{
    public void Serialize(ref MessagePackWriter writer, ExitCode value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        // Simplified example: Serialize type discriminator and properties directly
        writer.WriteMapHeader(3);
        writer.Write("Type");
        writer.Write(value.GetType().Name);
        writer.Write("Code");
        writer.Write(value.Code);
        writer.Write("Description");
        writer.Write(value.Description);
    }

    public ExitCode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null!;

        var count = reader.ReadMapHeader();
        string? typeName = null;

        for (var i = 0; i < count; i++)
        {
            var propertyName = reader.ReadString();
            switch (propertyName)
            {
                case "Type":
                    typeName = reader.ReadString();
                    break;
                case "Code":
                    reader.ReadInt32();
                    break;
                case "Description":
                    reader.ReadString();
                    break;
            }
        }

        // Instantiate based on type discriminator
        if (typeName == nameof(SuccessExitCode))
        {
            return new SuccessExitCode(); // Assuming SuccessExitCode fits this simple case
        }
        else
        {
            // Handle other types or throw an error if the type is unknown
            throw new InvalidOperationException($"Unsupported type: {typeName}");
        }
    }
}