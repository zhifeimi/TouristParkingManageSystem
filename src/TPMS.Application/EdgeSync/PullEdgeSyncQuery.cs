using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed record PullEdgeSyncQuery(Guid EdgeNodeId) : IQuery<Result<EdgeSyncBatchDto>>;
