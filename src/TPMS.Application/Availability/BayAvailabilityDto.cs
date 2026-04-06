namespace TPMS.Application.Availability;

public sealed record BayAvailabilityDto(
    Guid BayId,
    string BayNumber,
    string BayType,
    bool IsAvailable,
    bool IsReserved,
    bool IsOccupied,
    bool IsUnderMaintenance,
    string? OccupiedByLicensePlate);
