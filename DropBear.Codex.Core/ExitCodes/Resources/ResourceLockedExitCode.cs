using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Resources;
[MessagePackObject]
public class ResourceLockedExitCode() : ExitCode(423, "Resource is locked.");
