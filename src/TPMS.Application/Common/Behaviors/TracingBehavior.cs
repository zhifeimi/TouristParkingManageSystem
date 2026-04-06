using MediatR;

namespace TPMS.Application.Common.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity(typeof(TRequest).Name);
        activity?.SetTag("tpms.request.type", typeof(TRequest).FullName);

        return await next();
    }
}
