using MediatR;
using Microsoft.Extensions.Logging;

namespace TPMS.Application.Common.Behaviors;

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
        logger.LogInformation("Handling request {RequestName} with payload {@Request}", typeof(TRequest).Name, request);

        var response = await next();

        logger.LogInformation("Handled request {RequestName}", typeof(TRequest).Name);

        return response;
    }
}
