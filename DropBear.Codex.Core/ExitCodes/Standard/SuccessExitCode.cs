using DropBear.Codex.Core.ExitCodes.Base;

namespace DropBear.Codex.Core.ExitCodes.Standard;

public class SuccessExitCode : ExitCode
{
    public SuccessExitCode() : base(0, "Operation completed successfully.")
    {
    }
}