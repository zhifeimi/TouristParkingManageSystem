namespace TPMS.Edge.Persistence;

public sealed class LocalViolationRecord
{
    public Guid ViolationCaseId { get; set; }

    public Guid ParkingLotId { get; set; }

    public Guid? ParkingBayId { get; set; }

    public string? BayNumber { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? SyncedAtUtc { get; set; }
}
