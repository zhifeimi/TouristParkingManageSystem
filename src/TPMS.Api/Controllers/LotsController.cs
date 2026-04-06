using MediatR;
using Microsoft.AspNetCore.Mvc;
using TPMS.Application.Availability;
using TPMS.Application.Lots;

namespace TPMS.Api.Controllers;

[ApiController]
[Route("api/lots")]
public sealed class LotsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLots(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLotsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{lotId:guid}/bays/availability")]
    public async Task<IActionResult> GetAvailability(Guid lotId, [FromQuery] DateTimeOffset startUtc, [FromQuery] DateTimeOffset endUtc, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLotAvailabilityQuery(lotId, startUtc, endUtc), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
