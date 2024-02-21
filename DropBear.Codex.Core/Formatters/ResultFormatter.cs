using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

// Custom formatter for Result class
namespace DropBear.Codex.Core.Formatters;

public class ResultFormatter : IMessagePackFormatter<Result>
{
    public void Serialize(ref MessagePackWriter writer, Result value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        // Assuming ExitCode is already handled by its custom formatter
        // Write map header for 2 elements (ExitCode, ErrorMessage)
        writer.WriteMapHeader(2);

        // Serialize ExitCode
        writer.WriteInt32(0); // Key for ExitCode
        options.Resolver.GetFormatterWithVerify<ExitCode>().Serialize(ref writer, value.ExitCode, options);

        // Serialize ErrorMessage
        writer.WriteInt32(1); // Key for ErrorMessage
        writer.Write(value.ErrorMessage);
    }

    public Result Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        // Expect a map of 2 elements
        var count = reader.ReadMapHeader();
        ExitCode exitCode = null;
        string errorMessage = string.Empty;

        for (int i = 0; i < count; i++)
        {
            var key = reader.ReadInt32();
            switch (key)
            {
                case 0: // ExitCode
                    exitCode = options.Resolver.GetFormatterWithVerify<ExitCode>().Deserialize(ref reader, options);
                    break;
                case 1: // ErrorMessage
                    errorMessage = reader.ReadString();
                    break;
                default:
                    reader.Skip(); // Skip unknown fields
                    break;
            }
        }

        reader.Depth--;

        // Use protected constructor to instantiate Result
        return new Result(exitCode, errorMessage);
    }
}