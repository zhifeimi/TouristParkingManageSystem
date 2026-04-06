using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed class PullEdgeSyncQueryHandler(IEdgeSyncService edgeSyncService)
    : IRequestHandler<PullEdgeSyncQuery, Result<EdgeSyncBatchDto>>
{
    public async Task<Result<EdgeSyncBatchDto>> Handle(PullEdgeSyncQuery request, CancellationToken cancellationToken)
    {
        var batch = await edgeSyncService.CreatePullBatchAsync(request.EdgeNodeId, cancellationToken);
        return Result<EdgeSyncBatchDto>.Success(batch);
    }
}
