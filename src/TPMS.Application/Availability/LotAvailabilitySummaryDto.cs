namespace TPMS.Application.Availability;

public sealed record LotAvailabilitySummaryDto(
    Guid LotId,
    string LotName,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    int TotalBays,
    int AvailableBays,
    int OccupiedBays,
    int ReservedBays,
    IReadOnlyCollection<BayAvailabilityDto> Bays);
