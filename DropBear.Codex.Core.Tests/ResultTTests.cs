namespace DropBear.Codex.Core.Tests;

public class ResultTTests
{
    [Fact]
    public void Success_ReturnsResultWithIsSuccessTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ErrorMessage);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ReturnsResultWithIsSuccessFalse()
    {
        const string errorMessage = "Error occurred";
        var result = Result<int>.Failure(errorMessage);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void OnSuccess_ExecutesActionWhenIsSuccess()
    {
        var result = Result<int>.Success(42);
        var actionExecuted = false;
        result.OnSuccess(() => actionExecuted = true);
        Assert.True(actionExecuted);
    }

    [Fact]
    public void OnFailure_ExecutesActionWithErrorMessageWhenIsFailure()
    {
        const string errorMessage = "Error occurred";
        var result = Result<int>.Failure(errorMessage);
        var receivedMessage = string.Empty;
        result.OnFailure((msg, ex) => { receivedMessage = msg + (ex == null ? "" : ex.Message); });
        Assert.Equal(errorMessage, receivedMessage);
    }

    [Fact]
    public void Try_ReturnsSuccessResultWhenFunctionSucceeds()
    {
        var result = Result<int>.Try(() => 42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_ReturnsFailureResultWhenFunctionThrows()
    {
        var result = Result<int>.Try(() => throw new InvalidOperationException("Test exception"));
        Assert.False(result.IsSuccess);
        Assert.Contains("Test exception", result.ErrorMessage);
    }

    [Fact]
    public void Match_ExecutesCorrectFunctionBasedOnResultState()
    {
        var successResult = Result<int>.Success(42);
        var failureResult = Result<int>.Failure("Error");

        var successValue = successResult.Match(
            value => value * 2,
            (error, _) => 0
        );
        Assert.Equal(84, successValue);

        var failureValue = failureResult.Match(
            value => value * 2,
            (error, _) => 0
        );
        Assert.Equal(0, failureValue);
    }

    [Fact]
    public void Bind_TransformsSuccessResult()
    {
        var result = Result<int>.Success(42);
        var transformedResult = result.Bind(x => Result<string>.Success(x.ToString()));
        Assert.True(transformedResult.IsSuccess);
        Assert.Equal("42", transformedResult.Value);
    }

    [Fact]
    public void Bind_PreservesFailureResult()
    {
        var result = Result<int>.Failure("Error");
        var transformedResult = result.Bind(x => Result<string>.Success(x.ToString()));
        Assert.False(transformedResult.IsSuccess);
        Assert.Equal("Error", transformedResult.ErrorMessage);
    }
}
