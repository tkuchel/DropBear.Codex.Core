using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Validation;
[MessagePackObject]
public class InvalidInputExitCode : ExitCode
{
    public InvalidInputExitCode() : base(-1200, "The input provided is invalid.")
    {
    }
}