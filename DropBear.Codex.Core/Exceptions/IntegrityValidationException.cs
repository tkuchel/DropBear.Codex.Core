namespace DropBear.Codex.Core.Exceptions;

/// <summary>
///     Represents errors that occur during integrity validation of a payload.
/// </summary>
public class IntegrityValidationException : Exception
{
    public IntegrityValidationException() { }

    public IntegrityValidationException(string message)
        : base(message)
    {
    }

    public IntegrityValidationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
