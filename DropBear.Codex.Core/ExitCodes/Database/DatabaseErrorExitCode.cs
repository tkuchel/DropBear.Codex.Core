using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Database;
[MessagePackObject]
public class DatabaseErrorExitCode() : ExitCode(500, "Database operation failed.");
