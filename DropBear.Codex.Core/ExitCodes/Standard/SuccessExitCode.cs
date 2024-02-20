using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;
[MessagePackObject]
public class SuccessExitCode : ExitCode
{
    public SuccessExitCode() : base(0, "Operation completed successfully.")
    {
    }
}