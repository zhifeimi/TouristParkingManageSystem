namespace TPMS.Edge.Persistence;

public sealed class LocalLprEvent
{
    public Guid Id { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public Guid ParkingLotId { get; set; }

    public Guid? ParkingBayId { get; set; }

    public string? BayNumber { get; set; }

    public DateTimeOffset ObservedAtUtc { get; set; }

    public bool PermitMatched { get; set; }

    public DateTimeOffset? SyncedAtUtc { get; set; }
}
