using MessagePack;

namespace DropBear.Codex.Core.ExitCodes.Base;

[MessagePackObject]
public abstract class ExitCode(int code, string description)
{
    [Key(0)]
    public int Code { get; } = code;

    [Key(1)]
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

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