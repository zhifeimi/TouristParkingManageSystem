namespace TPMS.Application.EdgeSync;

public sealed record EdgeLprEventDto(
    Guid EventId,
    string LicensePlate,
    Guid ParkingLotId,
    Guid? ParkingBayId,
    string? BayNumber,
    DateTimeOffset ObservedAtUtc,
    bool PermitMatched);
