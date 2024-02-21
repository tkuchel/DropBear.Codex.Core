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

        writer.WriteMapHeader(3);
        writer.WriteInt32(0);
        options.Resolver.GetFormatterWithVerify<ExitCode>().Serialize(ref writer, value.ExitCode, options);
        
        writer.WriteInt32(1);
        writer.Write(value.ErrorMessage);

        writer.WriteInt32(2);
        options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Value, options);
    }

    public Result<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        var count = reader.ReadMapHeader();
        ExitCode exitCode = null;
        T value = default;
        string errorMessage = string.Empty;

        for (int i = 0; i < count; i++)
        {
            var key = reader.ReadInt32();
            switch (key)
            {
                case 0:
                    exitCode = options.Resolver.GetFormatterWithVerify<ExitCode>().Deserialize(ref reader, options);
                    break;
                case 1:
                    errorMessage = reader.ReadString();
                    break;
                case 2:
                    value = options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
                    break;
            }
        }

        reader.Depth--;

        return new Result<T>(exitCode, value, errorMessage); // Assumes the existence of a suitable constructor
    }
}