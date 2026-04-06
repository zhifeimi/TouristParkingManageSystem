using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.Lots;

public sealed class GetLotsQueryHandler(IAvailabilityReadService availabilityReadService)
    : IRequestHandler<GetLotsQuery, Result<IReadOnlyCollection<LotListItemDto>>>
{
    public async Task<Result<IReadOnlyCollection<LotListItemDto>>> Handle(GetLotsQuery request, CancellationToken cancellationToken)
    {
        var lots = await availabilityReadService.GetLotsAsync(cancellationToken);
        return Result<IReadOnlyCollection<LotListItemDto>>.Success(lots);
    }
}
