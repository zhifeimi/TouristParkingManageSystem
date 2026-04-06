using TPMS.Domain.Common;

namespace TPMS.Domain.Events;

public sealed record ReservationConfirmedDomainEvent(
    Guid ReservationId,
    Guid ParkingLotId,
    Guid ParkingBayId,
    string BayNumber,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
