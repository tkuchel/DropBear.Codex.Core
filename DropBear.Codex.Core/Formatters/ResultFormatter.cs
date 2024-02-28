using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class ResultFormatter : IMessagePackFormatter<Result>
{
    public void Serialize(ref MessagePackWriter writer, Result value, MessagePackSerializerOptions options)
    {
        // Assuming Result has an ExitCode and ErrorMessage for simplicity
        writer.WriteMapHeader(2);

        writer.Write(nameof(Result.ExitCode));
        options.Resolver.GetFormatterWithVerify<ExitCode>().Serialize(ref writer, value.ExitCode, options);

        writer.Write(nameof(Result.ErrorMessage));
        writer.Write(value.ErrorMessage);
    }

    public Result Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null!;

        var count = reader.ReadMapHeader();
        ExitCode exitCode = null!;
        var errorMessage = string.Empty;

        for (var i = 0; i < count; i++)
        {
            var propertyName = reader.ReadString();
            switch (propertyName)
            {
                case nameof(Result.ExitCode):
                    exitCode = options.Resolver.GetFormatterWithVerify<ExitCode>().Deserialize(ref reader, options);
                    break;
                case nameof(Result.ErrorMessage):
                    errorMessage = reader.ReadString();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        if (errorMessage == null) throw new InvalidOperationException("Invalid Result data.");
        if (exitCode == null!) throw new InvalidOperationException("Invalid Result data.");
#pragma warning disable CS0618 // Type or member is obsolete
        return new Result(exitCode, errorMessage);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
