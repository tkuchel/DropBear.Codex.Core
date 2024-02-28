using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;

[MessagePackObject]
public class GeneralErrorExitCode() : ExitCode(-1, "A general error has occurred.");