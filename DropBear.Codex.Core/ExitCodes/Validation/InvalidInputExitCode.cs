using DropBear.Codex.Core.ExitCodes.Base;

namespace DropBear.Codex.Core.ExitCodes.Validation;

public class InvalidInputExitCode : ExitCode
{
    public InvalidInputExitCode() : base(-1200, "The input provided is invalid.")
    {
    }
}