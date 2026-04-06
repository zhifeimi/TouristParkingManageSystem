using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Application.EdgeSync;

public sealed class PushEdgeSyncCommandHandler(IEdgeSyncService edgeSyncService)
    : IRequestHandler<PushEdgeSyncCommand, Result<EdgeSyncBatchDto>>
{
    public Task<Result<EdgeSyncBatchDto>> Handle(PushEdgeSyncCommand request, CancellationToken cancellationToken)
    {
        return edgeSyncService.AcceptPushBatchAsync(request.EdgeNodeId, request.Batch, cancellationToken);
    }
}
