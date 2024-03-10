using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;
[MessagePackObject]
public class NotFoundExitCode() : ExitCode(404, "Resource not found.");
