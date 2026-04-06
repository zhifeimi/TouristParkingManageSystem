namespace TPMS.Application.Enforcement;

public sealed record ViolationDto(
    Guid ViolationCaseId,
    Guid ParkingLotId,
    Guid? ParkingBayId,
    string? BayNumber,
    string LicensePlate,
    string Reason,
    string Details,
    string Status,
    DateTimeOffset CreatedAtUtc);
