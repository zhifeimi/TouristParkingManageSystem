using TPMS.Domain.Common;

namespace TPMS.Domain.Events;

public sealed record ReservationHeldDomainEvent(
    Guid ReservationId,
    Guid ParkingLotId,
    Guid ParkingBayId,
    string BayNumber,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
