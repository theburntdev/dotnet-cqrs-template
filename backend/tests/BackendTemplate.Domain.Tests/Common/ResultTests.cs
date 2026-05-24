namespace BackendTemplate.Domain.Tests.Common;

using BackendTemplate.Domain.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_GivenValue_ThenIsSuccessTrue()
    {
        var result = Result<string>.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Success_GivenValue_ThenAccessingErrorThrows()
    {
        var result = Result<string>.Success("hello");

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Success_GivenValue_ThenAccessingKindThrows()
    {
        var result = Result<string>.Success("hello");

        Assert.Throws<InvalidOperationException>(() => _ = result.Kind);
    }

    [Fact]
    public void Failure_GivenErrorAndKind_ThenIsFailureTrue()
    {
        var result = Result<string>.Failure("not found", ErrorKind.NotFound);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("not found", result.Error);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public void Failure_GivenError_ThenAccessingValueThrows()
    {
        var result = Result<string>.Failure("error");

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_WhenKindNotSpecified_ThenDefaultsToValidation()
    {
        var result = Result<string>.Failure("invalid");

        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public void NonGenericSuccess_ThenIsSuccessTrue()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void NonGenericSuccess_ThenAccessingErrorThrows()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void NonGenericFailure_GivenErrorAndKind_ThenIsFailureTrue()
    {
        var result = Result.Failure("conflict", ErrorKind.Conflict);

        Assert.True(result.IsFailure);
        Assert.Equal("conflict", result.Error);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public void NonGenericFailure_WhenKindNotSpecified_ThenDefaultsToValidation()
    {
        var result = Result.Failure("invalid");

        Assert.Equal(ErrorKind.Validation, result.Kind);
    }
}
