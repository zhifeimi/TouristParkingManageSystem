namespace TPMS.Edge.Persistence;

public sealed class LocalOccupancyRecord
{
    public Guid BayId { get; set; }

    public string BayNumber { get; set; } = string.Empty;

    public string OccupancyStatus { get; set; } = "Unknown";

    public string? LicensePlate { get; set; }

    public DateTimeOffset ObservedAtUtc { get; set; }

    public DateTimeOffset? SyncedAtUtc { get; set; }
}
