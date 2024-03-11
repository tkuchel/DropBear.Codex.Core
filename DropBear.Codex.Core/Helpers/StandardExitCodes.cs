using DropBear.Codex.Core.ExitCodes.Base;
using DropBear.Codex.Core.ExitCodes.Standard;

namespace DropBear.Codex.Core.Helpers;

public static class StandardExitCodes
{
    public static readonly ExitCode? Success = new SuccessExitCode();
    public static readonly ExitCode? GeneralError = new GeneralErrorExitCode();
    public static readonly ExitCode UnspecifiedError = new UnspecifiedExceptionExitCode();
}