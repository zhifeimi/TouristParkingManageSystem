using TPMS.Application.Common;

namespace TPMS.Application.Availability;

public sealed record GetLotAvailabilityQuery(Guid LotId, DateTimeOffset StartUtc, DateTimeOffset EndUtc)
    : IQuery<Result<LotAvailabilitySummaryDto>>,
      IAvailabilityWindowRequest;
