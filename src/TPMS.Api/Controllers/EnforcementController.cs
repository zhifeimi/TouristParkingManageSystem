using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPMS.Application.Enforcement;

namespace TPMS.Api.Controllers;

[ApiController]
[Route("api/enforcement/violations")]
[Authorize(Roles = "Controller,Operations,Admin")]
public sealed class EnforcementController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RaiseViolation([FromBody] RaiseViolationCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
