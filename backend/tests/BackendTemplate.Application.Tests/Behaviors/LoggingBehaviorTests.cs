namespace BackendTemplate.Application.Tests.Behaviors;

using BackendTemplate.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class LoggingBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_Always_ThenCallsNext()
    {
        var logger = NullLogger<LoggingBehavior<TestRequest, string>>.Instance;
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var nextCalled = false;
        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.True(nextCalled);
    }
}
