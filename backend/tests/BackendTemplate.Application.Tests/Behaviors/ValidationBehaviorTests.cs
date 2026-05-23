using BackendTemplate.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private record TestRequest(string Name) : IRequest<string>;

    private class TestValidator : AbstractValidator<TestRequest>
    {
        public TestValidator() => RuleFor(x => x.Name).NotEmpty();
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestValidator()]);
        var called = false;

        await behavior.Handle(
            new TestRequest("valid"),
            _ => { called = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task Handle_GivenInvalidRequest_ThenThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new TestValidator()]);

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(""),
                _ => Task.FromResult("ok"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GivenNoValidators_ThenCallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var called = false;

        await behavior.Handle(
            new TestRequest(""),
            _ => { called = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        Assert.True(called);
    }
}
