using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Permission;
[MessagePackObject]
public class PermissionDeniedExitCode() : ExitCode(403, "Permission denied.");
