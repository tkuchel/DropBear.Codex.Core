namespace DropBear.Codex.Core.Exceptions;

public class PayloadFormatterDeserializeException : Exception
{
    public PayloadFormatterDeserializeException() : base("Failed to deserialize payload")
    {
    }
    
    public PayloadFormatterDeserializeException(string message) : base(message)
    {
    }
    
    public PayloadFormatterDeserializeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}