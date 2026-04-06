namespace TPMS.Application.EdgeSync;

public sealed record EdgeOccupancyChangeDto(
    Guid BayId,
    string BayNumber,
    string OccupancyStatus,
    string? LicensePlate,
    DateTimeOffset ObservedAtUtc);
