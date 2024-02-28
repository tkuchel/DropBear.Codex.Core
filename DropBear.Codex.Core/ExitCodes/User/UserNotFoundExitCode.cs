using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.User;
[MessagePackObject]
public class UserNotFoundExitCode() : ExitCode(-1000, "The specified user was not found.");