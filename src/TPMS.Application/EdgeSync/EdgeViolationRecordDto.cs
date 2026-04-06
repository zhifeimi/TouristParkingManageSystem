namespace TPMS.Application.EdgeSync;

public sealed record EdgeViolationRecordDto(
    Guid ViolationCaseId,
    Guid ParkingLotId,
    Guid? ParkingBayId,
    string? BayNumber,
    string LicensePlate,
    string Reason,
    string Details,
    DateTimeOffset CreatedAtUtc);
