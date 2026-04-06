using TPMS.Domain.Common;
using TPMS.Domain.Enums;

namespace TPMS.Domain.Events;

public sealed record ReservationReassignedDomainEvent(
    Guid ReservationId,
    Guid ParkingLotId,
    Guid PreviousParkingBayId,
    Guid NewParkingBayId,
    string NewBayNumber,
    ReassignmentReason Reason,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
