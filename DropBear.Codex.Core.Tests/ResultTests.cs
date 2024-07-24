namespace DropBear.Codex.Core.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsResultWithIsSuccessTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void Failure_ReturnsResultWithIsSuccessFalse()
    {
        const string errorMessage = "Error occurred";
        var result = Result.Failure(errorMessage);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void OnSuccess_ExecutesActionWhenIsSuccess()
    {
        var result = Result.Success();
        var actionExecuted = false;
        result.OnSuccess(() => actionExecuted = true);
        Assert.True(actionExecuted);
    }

    [Fact]
    public void OnFailure_ExecutesActionWithErrorMessageWhenIsFailure()
    {
        const string errorMessage = "Error occurred";
        var result = Result.Failure(errorMessage);
        var receivedMessage = string.Empty;
        result.OnFailure((msg, ex) => { receivedMessage = msg + (ex == null ? "" : ex.Message); });
        Assert.Equal(errorMessage, receivedMessage);
    }
}
