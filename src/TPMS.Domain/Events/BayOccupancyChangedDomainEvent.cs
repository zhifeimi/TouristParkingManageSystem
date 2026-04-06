using TPMS.Domain.Common;
using TPMS.Domain.Enums;

namespace TPMS.Domain.Events;

public sealed record BayOccupancyChangedDomainEvent(
    Guid ParkingLotId,
    Guid ParkingBayId,
    string BayNumber,
    OccupancyStatus OccupancyStatus,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
