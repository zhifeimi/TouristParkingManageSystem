namespace TPMS.Application.Availability;

public sealed record BayOccupancySignal(
    Guid LotId,
    Guid BayId,
    string BayNumber,
    string OccupancyStatus,
    string? LicensePlate);
