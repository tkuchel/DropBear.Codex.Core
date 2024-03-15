using DropBear.Codex.Core.Models;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

public class ResultWithPayloadFormatter<T> : IMessagePackFormatter<ResultWithPayload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, ResultWithPayload<T> value,
        MessagePackSerializerOptions options)
    {
        // Initialize PayloadFormatter<T> for serializing the payload.
        var payloadFormatter = new PayloadFormatter<T>();

        writer.WriteMapHeader(value.IsSuccess ? 3 : 2);

        writer.Write(nameof(ResultWithPayload<T>.ErrorMessage));
        writer.Write(value.ErrorMessage);

        writer.Write(nameof(ResultWithPayload<T>.IsSuccess));
        writer.Write(value.IsSuccess);

        if (!value.IsSuccess || value.Payload is null) return;
        writer.Write("Payload");
        payloadFormatter.Serialize(ref writer, value.Payload, options);
    }

    public ResultWithPayload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null!;

        var count = reader.ReadMapHeader();
        var errorMessage = string.Empty;
        var isSuccess = false;
        Payload<T> payload = null!;

        // Initialize PayloadFormatter<T> for deserializing the payload.
        var payloadFormatter = new PayloadFormatter<T>();

        for (var i = 0; i < count; i++)
        {
            var propertyName = reader.ReadString();
            switch (propertyName)
            {
                case nameof(ResultWithPayload<T>.ErrorMessage):
                    errorMessage = reader.ReadString();
                    break;
                case nameof(ResultWithPayload<T>.IsSuccess):
                    isSuccess = reader.ReadBoolean();
                    break;
                case "Payload":
                    // Use PayloadFormatter<T> to deserialize the Payload object.
                    payload = payloadFormatter.Deserialize(ref reader, options);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        if (isSuccess && payload is not null) return ResultWithPayload.Success(payload.Data);

        return ResultWithPayload.Failure<T>(errorMessage ?? string.Empty);
    }
}
