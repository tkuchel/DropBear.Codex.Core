namespace DropBear.Codex.Core;

public enum ResultState
{
    Success,
    Failure,
    Pending, // Operation is still ongoing or its result is yet to be determined.
    Cancelled, // Operation was cancelled before completion.
    Warning, // Operation succeeded but with potential issues to check.
    PartialSuccess, // Operation succeeded partially.
    NoOp // No operation was performed.
}
