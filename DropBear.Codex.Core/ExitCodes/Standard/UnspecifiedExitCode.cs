using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;
[MessagePackObject]
public class UnspecifiedExceptionExitCode : ExitCode
{
    public UnspecifiedExceptionExitCode() : base(-2, "An unspecified exception occurred.")
    {
    }
}