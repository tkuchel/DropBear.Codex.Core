using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;
using Newtonsoft.Json;

namespace DropBear.Codex.Core.Formatters;

#pragma warning disable MA0048
public class ResultFormatter<T> : IMessagePackFormatter<Result<T>> where T : notnull
#pragma warning restore MA0048
{
    public void Serialize(ref MessagePackWriter writer, Result<T> value, MessagePackSerializerOptions options)
    {
        string? jsonValue = null;
        if (value.IsSuccess)
            try
            {
                jsonValue = JsonConvert.SerializeObject(value.Value);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate
                // Example: Log.Error(ex, "Failed to serialize Result<T>.Value to JSON.");
                jsonValue = "Error: Could not serialize value to JSON. " + ex.Message;
            }

        var headerSize = value.IsSuccess ? 3 : 2;
        writer.WriteMapHeader(headerSize);

        writer.Write(nameof(Result<T>.ExitCode));
        MessagePackSerializer.Serialize(ref writer, value.ExitCode, options);

        writer.Write(nameof(Result<T>.ErrorMessage));
        writer.Write(value.ErrorMessage);

        if (!value.IsSuccess) return;
        writer.Write("Value");
        writer.Write(jsonValue);
    }

    public Result<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions? options)
    {
        if (reader.TryReadNil())
            return null!;

        var count = reader.ReadMapHeader();
        ExitCode? exitCode = null;
        var errorMessage = string.Empty;
        T? value = default!;

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
                    var jsonValue = reader.ReadString();
                    try
                    {
                        value = (jsonValue is not null ? JsonConvert.DeserializeObject<T>(jsonValue) : default!)!;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as appropriate
                        // Example: Log.Error(ex, "Failed to deserialize JSON to Result<T>.Value.");
                        value = default;
                        errorMessage = "Error: Could not deserialize value from JSON. " + ex.Message;
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Unknown property: {propertyName}");
            }
        }

        return !string.IsNullOrEmpty(errorMessage)
            ? Result<T>.Failure(errorMessage, exitCode)
            : Result<T>.Success((value ?? default(T))!);
    }
}
