using MessagePack;

namespace DropBear.Codex.Core.ConsoleApp.Models;

[MessagePackObject]
public class TestDataTransferObject
{
    [Key(0)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Key(1)]
    public string Name { get; set; } = string.Empty;
}