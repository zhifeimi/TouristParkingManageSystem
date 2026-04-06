namespace TPMS.Application.EdgeSync;

public sealed record EdgeSyncBatchDto(
    Guid EdgeNodeId,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyCollection<EdgeSyncPermitDto> Permits,
    IReadOnlyCollection<EdgeOccupancyChangeDto> OccupancyChanges,
    IReadOnlyCollection<EdgeLprEventDto> LprEvents,
    IReadOnlyCollection<EdgeViolationRecordDto> Violations);
