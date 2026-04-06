using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPMS.Application.Availability;
using TPMS.Application.Reservations;

namespace TPMS.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return await ToActionResultAsync(result, command.LotId, command.StartUtc, command.EndUtc, cancellationToken);
    }

    [HttpGet("{reservationId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReservation(Guid reservationId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetReservationByIdQuery(reservationId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("{reservationId:guid}/reassign")]
    [Authorize(Roles = "Controller,Operations,Admin")]
    public async Task<IActionResult> ReassignReservation(Guid reservationId, [FromBody] ReassignBayRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ReassignBayCommand(reservationId, request.TargetBayId, request.Reason, request.Note),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }

    private async Task<IActionResult> ToActionResultAsync(
        Application.Common.Result<ReservationDto> result,
        Guid lotId,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetReservation), new { reservationId = result.Value!.ReservationId }, result.Value);
        }

        if (result.Error?.Code == "conflict")
        {
            var availability = await mediator.Send(new GetLotAvailabilityQuery(lotId, startUtc, endUtc), cancellationToken);
            return Conflict(new
            {
                result.Error,
                availability = availability.Value
            });
        }

        if (result.Error?.Code == "not_found")
        {
            return NotFound(result.Error);
        }

        return BadRequest(result.Error);
    }

    public sealed record ReassignBayRequest(Guid? TargetBayId, string Reason, string? Note);
}
