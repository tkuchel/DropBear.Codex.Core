using DropBear.Codex.Core.ExitCodes.Base;

namespace DropBear.Codex.Core.ExitCodes.Standard;

public class GeneralErrorExitCode : ExitCode
{
    public GeneralErrorExitCode() : base(-1, "A general error has occurred.")
    {
    }
}