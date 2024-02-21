using DropBear.Codex.Core.ConsoleApp.Models;
using DropBear.Codex.Core.Extensions;
using DropBear.Codex.Core.ReturnTypes;
using Kokuban;

namespace DropBear.Codex.Core.ConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine(Chalk.Blue + "[ℹ️] Starting DropBear.Codex.Core.ConsoleApp");

        var testObject1 = new TestDataTransferObject
        {
            Name = "Terrence Kuchel"
        };
        
        Console.WriteLine(Chalk.Gray + "[⏳] Test object created.");

        var newTestDataObject = ResultWithPayload<TestDataTransferObject>.Success(testObject1);
        
        Console.WriteLine(Chalk.Gray + "[⏳] Test ResultWithPayload object created.");
        
        var serializedWithChecksum = newTestDataObject.SerializeWithChecksum();
        
        Console.WriteLine(Chalk.Green + "[✅] Test ResultWithPayload object serialized with checksum.");
        
        var deserializedWithChecksum =
            ResultExtensions.DeserializeWithChecksum<TestDataTransferObject>(serializedWithChecksum);
        
        Console.WriteLine(Chalk.Green + "[✅] Test ResultWithPayload object deserialized with checksum.");
        
        Console.WriteLine(Chalk.Blue + "[ℹ️] Exiting DropBear.Codex.Core.ConsoleApp");
    }
}