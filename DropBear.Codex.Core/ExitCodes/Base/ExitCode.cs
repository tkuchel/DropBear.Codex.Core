using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Base;

[MessagePackObject]
public abstract class ExitCode
{
    protected ExitCode(int code, string description)
    {
        Code = code;
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    [Key(0)]
    public int Code { get; }
    
    [Key(1)]
    public string Description { get; }

    public override bool Equals(object? obj)
    {
        return obj is ExitCode other && Code == other.Code;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    
    public static bool operator ==(ExitCode left, ExitCode right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ExitCode left, ExitCode? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{Code}: {Description}";
    }
}