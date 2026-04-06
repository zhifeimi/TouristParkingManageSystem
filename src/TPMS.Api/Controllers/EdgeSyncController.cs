using MediatR;
using Microsoft.AspNetCore.Mvc;
using TPMS.Application.EdgeSync;

namespace TPMS.Api.Controllers;

[ApiController]
[Route("api/edge/sync")]
public sealed class EdgeSyncController(IMediator mediator) : ControllerBase
{
    [HttpGet("pull")]
    public async Task<IActionResult> Pull([FromQuery] Guid edgeNodeId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PullEdgeSyncQuery(edgeNodeId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromQuery] Guid edgeNodeId, [FromBody] EdgeSyncBatchDto batch, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PushEdgeSyncCommand(edgeNodeId, batch), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
