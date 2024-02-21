using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.User;
[MessagePackObject]
public class UserNotFoundExitCode : ExitCode
{
    public UserNotFoundExitCode() : base(-1000, "The specified user was not found.")
    {
    }
}