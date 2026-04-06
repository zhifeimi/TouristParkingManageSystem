using TPMS.Application.Common;
using TPMS.Application.EdgeSync;

namespace TPMS.Application.Abstractions;

public interface IEdgeSyncService
{
    Task<EdgeSyncBatchDto> CreatePullBatchAsync(Guid edgeNodeId, CancellationToken cancellationToken);

    Task<Result<EdgeSyncBatchDto>> AcceptPushBatchAsync(Guid edgeNodeId, EdgeSyncBatchDto batch, CancellationToken cancellationToken);
}
