namespace DropBear.Codex.Core.Tests;

public class ResultT1T2Tests
{
    [Fact]
    public void Success_ReturnsResultWithIsSuccessTrue()
    {
        var value1 = 42;
        var value2 = "value";
        var result = Result<int, string>.Succeeded(value1, value2);
        Assert.True(result.IsSuccess);
        Assert.Equal(value1, result.Success);
        Assert.Equal(value2, result.Failure);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ReturnsResultWithIsSuccessFalse()
    {
        var errorMessage = "Error occurred";
        var result = Result<int, string>.Failed(errorMessage);
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Success);
        Assert.Equal(default, result.Failure);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void OnSuccess_ExecutesActionWithValuesWhenIsSuccess()
    {
        var value1 = 42;
        var value2 = "value";
        var result = Result<int, string>.Succeeded(value1, value2);
        var receivedValue1 = 0;
        var receivedValue2 = string.Empty;
        result.OnSuccess((val1, val2) =>
        {
            receivedValue1 = val1;
            receivedValue2 = val2;
        });
        Assert.Equal(value1, receivedValue1);
        Assert.Equal(value2, receivedValue2);
    }

    [Fact]
    public void OnFailure_ExecutesActionWithErrorMessageWhenIsFailure()
    {
        var errorMessage = "Error occurred";
        var result = Result<int, string>.Failed(errorMessage);
        var receivedMessage = string.Empty;
        result.OnFailure(msg => receivedMessage = msg);
        Assert.Equal(errorMessage, receivedMessage);
    }
}