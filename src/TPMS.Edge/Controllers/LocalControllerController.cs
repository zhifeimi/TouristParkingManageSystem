using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPMS.Edge.Persistence;
using TPMS.Edge.Services;

namespace TPMS.Edge.Controllers;

[ApiController]
[Route("api/local")]
public sealed class LocalControllerController(
    EdgeDbContext dbContext,
    EdgeLprService edgeLprService) : ControllerBase
{
    [HttpGet("controller/dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var occupancy = await dbContext.Occupancy
            .AsNoTracking()
            .OrderBy(entity => entity.BayNumber)
            .ToListAsync(cancellationToken);

        var recentEvents = await dbContext.LprEvents
            .AsNoTracking()
            .OrderByDescending(entity => entity.ObservedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        var unsyncedCount = await dbContext.LprEvents.CountAsync(entity => entity.SyncedAtUtc == null, cancellationToken)
                           + await dbContext.Violations.CountAsync(entity => entity.SyncedAtUtc == null, cancellationToken);

        return Ok(new
        {
            Occupancy = occupancy,
            RecentLprEvents = recentEvents,
            UnsyncedCount = unsyncedCount
        });
    }

    [HttpGet("permits/validate/{licensePlate}")]
    public async Task<IActionResult> ValidatePermit(string licensePlate, CancellationToken cancellationToken)
    {
        var result = await edgeLprService.ValidatePermitAsync(licensePlate, cancellationToken);
        return Ok(result);
    }

    [HttpPost("lpr-events")]
    public async Task<IActionResult> RecordLprEvent([FromBody] RecordLprEventRequest request, CancellationToken cancellationToken)
    {
        await edgeLprService.RecordLprEventAsync(request.LicensePlate, request.BayId, request.BayNumber, cancellationToken);
        return Accepted();
    }

    [HttpPost("violations")]
    public async Task<IActionResult> RecordViolation([FromBody] RecordViolationRequest request, CancellationToken cancellationToken)
    {
        var violation = new LocalViolationRecord
        {
            ViolationCaseId = request.ViolationCaseId == Guid.Empty ? Guid.NewGuid() : request.ViolationCaseId,
            ParkingLotId = request.ParkingLotId,
            ParkingBayId = request.ParkingBayId,
            BayNumber = request.BayNumber,
            LicensePlate = request.LicensePlate,
            Reason = request.Reason,
            Details = request.Details,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await dbContext.Violations.AddAsync(violation, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(violation);
    }

    public sealed record RecordLprEventRequest(string LicensePlate, Guid? BayId, string? BayNumber);

    public sealed record RecordViolationRequest(
        Guid ViolationCaseId,
        Guid ParkingLotId,
        Guid? ParkingBayId,
        string? BayNumber,
        string LicensePlate,
        string Reason,
        string Details);
}
