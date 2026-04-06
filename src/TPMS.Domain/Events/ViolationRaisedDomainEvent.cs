using TPMS.Domain.Common;

namespace TPMS.Domain.Events;

public sealed record ViolationRaisedDomainEvent(
    Guid ViolationCaseId,
    Guid ParkingLotId,
    Guid? ParkingBayId,
    string LicensePlate,
    string Reason,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
