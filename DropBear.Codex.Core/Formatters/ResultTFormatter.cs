using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class ResultFormatter<T> : IMessagePackFormatter<Result<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, Result<T> value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var headerSize = value.IsSuccess ? 3 : 2; // Adjust based on whether T is included
        writer.WriteMapHeader(headerSize);

        writer.Write(nameof(Result<T>.ExitCode));
        MessagePackSerializer.Serialize(ref writer, value.ExitCode, options);

        writer.Write(nameof(Result<T>.ErrorMessage));
        writer.Write(value.ErrorMessage);

        if (value.IsSuccess)
        {
            writer.Write("Value");
            MessagePackSerializer.Serialize(ref writer, value.Value, options);
        }
    }

    public Result<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;

        var count = reader.ReadMapHeader();
        ExitCode exitCode = null;
        var errorMessage = string.Empty;
        T value = default;

        for (var i = 0; i < count; i++)
        {
            var propertyName = reader.ReadString();
            switch (propertyName)
            {
                case nameof(Result<T>.ExitCode):
                    exitCode = MessagePackSerializer.Deserialize<ExitCode>(ref reader, options);
                    break;
                case nameof(Result<T>.ErrorMessage):
                    errorMessage = reader.ReadString();
                    break;
                case "Value":
                    value = MessagePackSerializer.Deserialize<T>(ref reader, options);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        if (value != null)
            return Result<T>.Success(value);
        return Result<T>.Failure(errorMessage, exitCode);
    }
}