namespace TPMS.Application.EdgeSync;

public sealed record EdgeSyncPermitDto(
    Guid PermitId,
    Guid ReservationId,
    string PermitCode,
    string LicensePlate,
    string BayNumber,
    DateTimeOffset ValidFromUtc,
    DateTimeOffset ValidToUtc,
    string Status);
