using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;

[MessagePackObject]
public class GeneralErrorExitCode : ExitCode
{
    public GeneralErrorExitCode() : base(-1, "A general error has occurred.")
    {
    }
}