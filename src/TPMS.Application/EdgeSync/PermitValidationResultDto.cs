namespace TPMS.Application.EdgeSync;

public sealed record PermitValidationResultDto(
    string LicensePlate,
    bool IsValid,
    string? PermitCode,
    string? BayNumber,
    DateTimeOffset? ValidToUtc,
    string Status,
    string Message);
