using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            sw.Stop();
            logger.LogInformation("{RequestName} completed in {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);
        }
    }
}
