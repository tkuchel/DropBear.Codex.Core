namespace DropBear.Codex.Core.Tests;

public class ResultT1T2Tests
{
    [Fact]
    public void Succeeded_ReturnsResultWithIsSuccessTrue()
    {
        var value = 42;
        var result = Result<int, string>.Succeeded(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Success);
        Assert.Throws<InvalidOperationException>(() => result.Failure);
    }

    [Fact]
    public void Failed_ReturnsResultWithIsSuccessFalse()
    {
        var errorValue = "Error occurred";
        var result = Result<int, string>.Failed(errorValue);

        Assert.False(result.IsSuccess);
        Assert.Throws<InvalidOperationException>(() => result.Success);
        Assert.Equal(errorValue, result.Failure);
    }

    [Fact]
    public void Match_ExecutesSuccessActionWhenIsSuccess()
    {
        var value = 42;
        var result = Result<int, string>.Succeeded(value);
        var receivedValue = 0;

        result.Match(
            val => receivedValue = val,
            _ => { }
        );

        Assert.Equal(value, receivedValue);
    }

    [Fact]
    public void Match_ExecutesFailureActionWhenIsFailure()
    {
        var errorValue = "Error occurred";
        var result = Result<int, string>.Failed(errorValue);
        var receivedError = string.Empty;

        result.Match(
            _ => { },
            error => receivedError = error
        );

        Assert.Equal(errorValue, receivedError);
    }

    [Fact]
    public void Bind_TransformsSuccessResult()
    {
        var initialValue = 42;
        var result = Result<int, string>.Succeeded(initialValue);

        var transformedResult = result.Bind(x => Result<string, string>.Succeeded(x.ToString()));

        Assert.True(transformedResult.IsSuccess);
        Assert.Equal(initialValue.ToString(), transformedResult.Success);
    }

    [Fact]
    public void Bind_PreservesFailureResult()
    {
        var errorValue = "Initial error";
        var result = Result<int, string>.Failed(errorValue);

        var transformedResult = result.Bind(x => Result<string, string>.Succeeded(x.ToString()));

        Assert.False(transformedResult.IsSuccess);
        Assert.Equal(errorValue, transformedResult.Failure);
    }

    [Fact]
    public void Map_TransformsSuccessValue()
    {
        var initialValue = 42;
        var result = Result<int, string>.Succeeded(initialValue);

        var mappedResult = result.Map(x => x.ToString());

        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(initialValue.ToString(), mappedResult.Success);
    }

    [Fact]
    public void Map_PreservesFailureResult()
    {
        var errorValue = "Error occurred";
        var result = Result<int, string>.Failed(errorValue);

        var mappedResult = result.Map(x => x.ToString());

        Assert.False(mappedResult.IsSuccess);
        Assert.Equal(errorValue, mappedResult.Failure);
    }

    [Fact]
    public void ImplicitConversion_FromSuccessValue_CreatesSuccessResult()
    {
        var successValue = 42;
        Result<int, string> result = successValue;

        Assert.True(result.IsSuccess);
        Assert.Equal(successValue, result.Success);
    }

    [Fact]
    public void ImplicitConversion_FromFailureValue_CreatesFailureResult()
    {
        var failureValue = "Error occurred";
        Result<int, string> result = failureValue;

        Assert.False(result.IsSuccess);
        Assert.Equal(failureValue, result.Failure);
    }
}
