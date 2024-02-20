using DropBear.Codex.Core.ExitCodes.Base;

namespace DropBear.Codex.Core.ExitCodes.Standard;

public class UnspecifiedExceptionExitCode : ExitCode
{
    public UnspecifiedExceptionExitCode() : base(-2, "An unspecified exception occurred.")
    {
    }
}