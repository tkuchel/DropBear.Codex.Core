using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;
using System;
using DropBear.Codex.Core.Models;

namespace DropBear.Codex.Core.Formatters;

public class ResultWithPayloadFormatter<T> : IMessagePackFormatter<ResultWithPayload<T>> where T : notnull
{
    public void Serialize(ref MessagePackWriter writer, ResultWithPayload<T> value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteMapHeader(value.IsSuccess ? 3 : 2);

        writer.Write(nameof(ResultWithPayload<T>.ErrorMessage));
        writer.Write(value.ErrorMessage);

        writer.Write(nameof(ResultWithPayload<T>.IsSuccess));
        writer.Write(value.IsSuccess);

        if (value.IsSuccess && value.Payload != null)
        {
            writer.Write("Payload");
            // Given Payload<T> encapsulates T, serialize the Payload<T> directly
            MessagePackSerializer.Serialize(ref writer, value.Payload, options);
        }
    }

    public ResultWithPayload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null;

        var count = reader.ReadMapHeader();
        var errorMessage = string.Empty;
        var isSuccess = false;
        Payload<T> payload = null;

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
                    // Deserialize the Payload<T> object directly
                    payload = MessagePackSerializer.Deserialize<Payload<T>>(ref reader, options);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        if (isSuccess && payload != null)
        {
            // Use the deserialized Payload<T> to create a successful ResultWithPayload<T>
            return ResultWithPayload<T>.Success(payload.Data);
        }
        else
        {
            return ResultWithPayload<T>.Failure(errorMessage);
        }
    }
}
