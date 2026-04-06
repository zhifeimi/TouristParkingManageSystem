namespace TPMS.Edge.Persistence;

public sealed class LocalPermitCacheItem
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }

    public string PermitCode { get; set; } = string.Empty;

    public string LicensePlate { get; set; } = string.Empty;

    public string BayNumber { get; set; } = string.Empty;

    public DateTimeOffset ValidFromUtc { get; set; }

    public DateTimeOffset ValidToUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset SyncedAtUtc { get; set; }
}
