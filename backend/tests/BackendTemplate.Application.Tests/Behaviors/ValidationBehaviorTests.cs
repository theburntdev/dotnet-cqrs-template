namespace BackendTemplate.Application.Tests.Behaviors;

using BackendTemplate.Application.Behaviors;
using FluentValidation;
using MediatR;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_GivenRequestWithNoValidators_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestRequestValidator()]);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("hello"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_GivenInvalidRequest_ThenThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestRequestValidator()]);
        Task<string> Next() => Task.FromResult("ok");

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(""), Next, CancellationToken.None));
    }
}
