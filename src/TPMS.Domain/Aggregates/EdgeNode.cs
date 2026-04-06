using TPMS.Domain.Common;
using TPMS.Domain.Enums;

namespace TPMS.Domain.Aggregates;

public sealed class EdgeNode : AggregateRoot<Guid>
{
    private EdgeNode()
    {
    }

    public EdgeNode(Guid id, Guid parkingLotId, string nodeCode)
        : base(id)
    {
        ParkingLotId = parkingLotId;
        NodeCode = nodeCode.Trim().ToUpperInvariant();
        SyncStatus = SyncStatus.Offline;
    }

    public Guid ParkingLotId { get; private set; }

    public string NodeCode { get; private set; } = string.Empty;

    public SyncStatus SyncStatus { get; private set; }

    public DateTimeOffset? LastSeenAtUtc { get; private set; }

    public DateTimeOffset? LastSuccessfulSyncAtUtc { get; private set; }

    public int PendingEventCount { get; private set; }

    public void MarkHeartbeat(DateTimeOffset nowUtc, SyncStatus status)
    {
        LastSeenAtUtc = nowUtc;
        SyncStatus = status;
    }

    public void MarkSync(DateTimeOffset nowUtc, int pendingEventCount)
    {
        LastSuccessfulSyncAtUtc = nowUtc;
        PendingEventCount = pendingEventCount;
        SyncStatus = pendingEventCount == 0 ? SyncStatus.Online : SyncStatus.Degraded;
    }
}
