using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed record PushEdgeSyncCommand(Guid EdgeNodeId, EdgeSyncBatchDto Batch)
    : ICommand<Result<EdgeSyncBatchDto>>;
