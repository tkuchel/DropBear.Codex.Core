using System.Net;
using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ExitCodes.Standard;

namespace DropBear.Codex.Core.Utilities;

public static class ExitCodeHttpStatusMapper
{
    private static readonly Dictionary<Type, HttpStatusCode> Map = new()
    {
        { typeof(SuccessExitCode), HttpStatusCode.OK },
        { typeof(GeneralErrorExitCode), HttpStatusCode.BadRequest },
        { typeof(UnspecifiedExceptionExitCode), HttpStatusCode.InternalServerError },
        // Add mappings for other exit codes as necessary
    };

    public static HttpStatusCode GetStatusCode(ExitCode exitCode)
    {
        ArgumentNullException.ThrowIfNull(exitCode);

        return Map.GetValueOrDefault(exitCode.GetType(), HttpStatusCode.InternalServerError);
    }
}