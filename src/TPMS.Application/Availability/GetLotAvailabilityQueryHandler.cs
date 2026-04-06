using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.Availability;

public sealed class GetLotAvailabilityQueryHandler(IAvailabilityReadService availabilityReadService)
    : IRequestHandler<GetLotAvailabilityQuery, Result<LotAvailabilitySummaryDto>>
{
    public async Task<Result<LotAvailabilitySummaryDto>> Handle(GetLotAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var summary = await availabilityReadService.GetLotAvailabilityAsync(
            request.LotId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);

        return summary is null
            ? Result<LotAvailabilitySummaryDto>.NotFound("Parking lot was not found.")
            : Result<LotAvailabilitySummaryDto>.Success(summary);
    }
}
