using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Validation;

[MessagePackObject]
public class ValidationErrorExitCode() : ExitCode(400, "Validation error occurred.");
