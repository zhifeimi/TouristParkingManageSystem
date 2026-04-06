using MediatR;

namespace TPMS.Application.Common.Behaviors;

public sealed class ConcurrencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (ConcurrencyConflictException ex) when (request is IAvailabilityWindowRequest availabilityRequest)
        {
            throw new AvailabilityConcurrencyException(
                ex.Message,
                availabilityRequest.LotId,
                availabilityRequest.StartUtc,
                availabilityRequest.EndUtc,
                ex);
        }
    }
}
