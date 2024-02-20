using DropBear.Codex.Core.ExitCodes.Base;

namespace DropBear.Codex.Core.ExitCodes.User;

public class UserNotFoundExitCode : ExitCode
{
    public UserNotFoundExitCode() : base(-1000, "The specified user was not found.")
    {
    }
}