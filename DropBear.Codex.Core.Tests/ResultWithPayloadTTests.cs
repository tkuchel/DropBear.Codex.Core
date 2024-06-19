namespace DropBear.Codex.Core.Tests;

public class ResultWithPayloadTTests
{
    [Fact]
    public void Success_ReturnsResultWithIsSuccessTrue()
    {
        var payload = new TestClass { Name = "Test", Age = 42 };
        var result = ResultWithPayload<TestClass>.SuccessWithPayload(payload);
        Assert.True(result.State == ResultState.Success);
        var decompressedResult = result.DecompressAndDeserialize();
        Assert.True(decompressedResult.IsSuccess, decompressedResult.ErrorMessage);
        Assert.Equal(payload.Name, decompressedResult.Value?.Name);
        Assert.Equal(payload.Age, decompressedResult.Value?.Age);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ReturnsResultWithIsSuccessFalse()
    {
        const string errorMessage = "Error occurred";
        var result = ResultWithPayload<string>.FailureWithPayload(errorMessage);
        Assert.False(result.State == ResultState.Success);
        var decompressedResult = result.DecompressAndDeserialize();
        Assert.False(decompressedResult.IsSuccess);
        Assert.Equal("Operation failed, cannot decompress.", decompressedResult.ErrorMessage);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    private sealed class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}