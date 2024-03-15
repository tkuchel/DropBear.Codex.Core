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
        // Assuming 2 always present fields: IsSuccess and ErrorMessage, and optionally Payload if IsSuccess is true
        writer.WriteMapHeader(value is { IsSuccess: true, Payload: not null } ? 3 : 2);

        writer.Write(nameof(ResultWithPayload<T>.IsSuccess));
        writer.Write(value.IsSuccess);

        writer.Write(nameof(ResultWithPayload<T>.ErrorMessage));
        writer.Write(value.ErrorMessage);

        // Serialize the payload if present and successful.
        if (value is not { IsSuccess: true, Payload: not null }) return;
        writer.Write("Payload");

        // Direct serialization of Payload using MessagePackSerializer, utilizing its options for potential custom formatters.
        MessagePackSerializer.Serialize(ref writer, value.Payload, options);
    }

    public ResultWithPayload<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadMapHeader();
        var isSuccess = false;
        var errorMessage = string.Empty;
        Payload<T>? payload = null;

        for (var i = 0; i < count; i++)
        {
            var propertyName = reader.ReadString();
            switch (propertyName)
            {
                case nameof(ResultWithPayload<T>.IsSuccess):
                    isSuccess = reader.ReadBoolean();
                    break;
                case nameof(ResultWithPayload<T>.ErrorMessage):
                    errorMessage = reader.ReadString();
                    break;
                case "Payload":
                    // Direct deserialization of Payload using MessagePackSerializer, utilizing its options for potential custom formatters.
                    payload = MessagePackSerializer.Deserialize<Payload<T>>(ref reader, options);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        // Using factory method for consistency with how instances are expected to be created.
        if (isSuccess && payload is not null)
            return ResultWithPayloadFactory.Success(payload.Data, payload._checksum, payload._signature,
                payload._publicKey, payload.Timestamp, payload.DataType, payload.PayloadId);
        return ResultWithPayloadFactory.Failure<T>(errorMessage ?? "Unknown error occurred.");
    }
}
