﻿using DropBear.Codex.Core.ExitCodes.Base;
using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Standard;
[MessagePackObject]
public class SuccessExitCode() : ExitCode(0, "Operation completed successfully.");