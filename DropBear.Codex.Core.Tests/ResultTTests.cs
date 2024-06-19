namespace DropBear.Codex.Core.Tests;

public class ResultTTests
{
    [Fact]
    public void Success_ReturnsResultWithIsSuccessTrue()
    {
        const int value = 42;
        var result = Result<int>.Success(value);
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ReturnsResultWithIsSuccessFalse()
    {
        const string errorMessage = "Error occurred";
        var result = Result<int>.Failure(errorMessage);
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void OnSuccess_ExecutesActionWithValueWhenIsSuccess()
    {
        const int value = 42;
        var result = Result<int>.Success(value);
        var receivedValue = 0;
        _ = result.OnSuccess(val =>
        {
            receivedValue = val;
            return Result<int>.Success(receivedValue);
        });
        Assert.Equal(value, receivedValue);
    }

    [Fact]
    public void OnFailure_ExecutesActionWithErrorMessageWhenIsFailure()
    {
        const string errorMessage = "Error occurred";
        var result = Result<int>.Failure(errorMessage);
        string receivedMessage = string.Empty;
        result.OnFailure((msg, ex) => { receivedMessage = msg + (ex == null ? "" : ex.Message); });
        Assert.Equal(errorMessage, receivedMessage);
    }
}