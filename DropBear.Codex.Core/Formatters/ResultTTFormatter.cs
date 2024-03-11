using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.Helpers;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Core.Formatters;

#pragma warning disable MA0048
public class ResultT1T2Formatter<T1, T2> : IMessagePackFormatter<Result<T1, T2>>
#pragma warning restore MA0048
{
    public void Serialize(ref MessagePackWriter writer, Result<T1, T2> value, MessagePackSerializerOptions options)
    {
        // Adjust the map header to include three elements
        writer.WriteMapHeader(3);

        // Serialize the ExitCode
        writer.Write(nameof(Result<T1, T2>.ExitCode));
        options.Resolver.GetFormatterWithVerify<ExitCode>().Serialize(ref writer, value.ExitCode ?? StandardExitCodes.UnspecifiedError, options);

        // Serialize the IsSuccess flag
        writer.Write(nameof(Result<T1, T2>.IsSuccess));
        writer.Write(value.IsSuccess);

        // Conditionally serialize the SuccessValue or FailureValue
        if (value.IsSuccess)
        {
            writer.Write("SuccessValue");
            if (value.SuccessValue is not null)
                options.Resolver.GetFormatterWithVerify<T1>().Serialize(ref writer, value.SuccessValue, options);
        }
        else
        {
            writer.Write("FailureValue");
            if (value.FailureValue is not null)
                options.Resolver.GetFormatterWithVerify<T2>().Serialize(ref writer, value.FailureValue, options);
        }
    }

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
    public Result<T1?, T2?> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
    {
        var length = reader.ReadMapHeader();
        if (length is not 3)
            throw new MessagePackSerializationException("Invalid Result<T1, T2> format.");

        // Initialize default values
        var exitCode = StandardExitCodes.Success; // Default exit code
        T1? successValue = default;
        T2? failureValue = default;
        var isSuccess = false;

        for (var i = 0; i < length; i++)
        {
            var key = reader.ReadString();
            switch (key)
            {
                case nameof(Result<T1, T2>.ExitCode):
                    exitCode = options.Resolver.GetFormatterWithVerify<ExitCode>().Deserialize(ref reader, options);
                    break;
                case nameof(Result<T1, T2>.IsSuccess):
                    isSuccess = reader.ReadBoolean();
                    break;
                case "SuccessValue":
                    successValue = options.Resolver.GetFormatterWithVerify<T1>().Deserialize(ref reader, options);
                    break;
                case "FailureValue":
                    failureValue = options.Resolver.GetFormatterWithVerify<T2>().Deserialize(ref reader, options);
                    break;
            }
        }

        return isSuccess ? new Result<T1?, T2?>(successValue) : new Result<T1?, T2?>(failureValue,exitCode);
    }
}
