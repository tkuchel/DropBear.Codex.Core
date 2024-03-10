using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Authentication;
[MessagePackObject]
public class AuthenticationFailedExitCode() : ExitCode(401, "Authentication failed.");
