using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;
using TPMS.Application.EdgeSync;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;
using TPMS.Infrastructure.Persistence;

namespace TPMS.Infrastructure.Realtime;

public sealed class EdgeSyncService(
    TpmsDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IEdgeSyncService
{
    public async Task<EdgeSyncBatchDto> CreatePullBatchAsync(Guid edgeNodeId, CancellationToken cancellationToken)
    {
        var permits = await dbContext.Permits
            .AsNoTracking()
            .OrderByDescending(entity => entity.ValidToUtc)
            .Take(200)
            .Select(entity => new EdgeSyncPermitDto(
                entity.Id,
                entity.ReservationId,
                entity.PermitCode,
                entity.LicensePlate.Value,
                entity.BayNumber.Value,
                entity.ValidFromUtc,
                entity.ValidToUtc,
                entity.Status.ToString()))
            .ToListAsync(cancellationToken);

        var occupancyChanges = await dbContext.ParkingBays
            .AsNoTracking()
            .Where(entity => entity.OccupancyStatus != OccupancyStatus.Unknown)
            .Select(entity => new EdgeOccupancyChangeDto(
                entity.Id,
                entity.BayNumber.Value,
                entity.OccupancyStatus.ToString(),
                entity.OccupiedByLicensePlate,
                entity.AvailabilityTouchedAtUtc))
            .ToListAsync(cancellationToken);

        var violations = await dbContext.ViolationCases
            .AsNoTracking()
            .OrderByDescending(entity => entity.CreatedAtUtc)
            .Take(50)
            .Select(entity => new EdgeViolationRecordDto(
                entity.Id,
                entity.ParkingLotId,
                entity.ParkingBayId,
                entity.BayNumber == null ? null : entity.BayNumber.Value,
                entity.LicensePlate.Value,
                entity.Reason,
                entity.Details,
                entity.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var edgeNode = await dbContext.EdgeNodes.SingleOrDefaultAsync(entity => entity.Id == edgeNodeId, cancellationToken);
        if (edgeNode is not null)
        {
            edgeNode.MarkHeartbeat(dateTimeProvider.UtcNow, SyncStatus.Online);
        }

        return new EdgeSyncBatchDto(
            edgeNodeId,
            dateTimeProvider.UtcNow,
            permits,
            occupancyChanges,
            [],
            violations);
    }

    public async Task<Result<EdgeSyncBatchDto>> AcceptPushBatchAsync(Guid edgeNodeId, EdgeSyncBatchDto batch, CancellationToken cancellationToken)
    {
        var edgeNode = await dbContext.EdgeNodes.SingleOrDefaultAsync(entity => entity.Id == edgeNodeId, cancellationToken);
        if (edgeNode is null)
        {
            var firstLotId = await dbContext.ParkingLots.AsNoTracking().Select(entity => entity.Id).FirstOrDefaultAsync(cancellationToken);
            if (firstLotId == Guid.Empty)
            {
                return Result<EdgeSyncBatchDto>.NotFound("No parking lot is available for edge sync.");
            }

            edgeNode = new EdgeNode(edgeNodeId, firstLotId, $"EDGE-{edgeNodeId.ToString("N")[..6].ToUpperInvariant()}");
            await dbContext.EdgeNodes.AddAsync(edgeNode, cancellationToken);
        }

        foreach (var incomingViolation in batch.Violations)
        {
            var exists = await dbContext.ViolationCases.AnyAsync(entity => entity.Id == incomingViolation.ViolationCaseId, cancellationToken);
            if (exists)
            {
                continue;
            }

            var violation = new ViolationCase(
                incomingViolation.ViolationCaseId,
                incomingViolation.ParkingLotId,
                incomingViolation.ParkingBayId,
                string.IsNullOrWhiteSpace(incomingViolation.BayNumber) ? null : new BayNumber(incomingViolation.BayNumber),
                new LicensePlate(incomingViolation.LicensePlate),
                incomingViolation.Reason,
                incomingViolation.Details,
                incomingViolation.CreatedAtUtc);

            await dbContext.ViolationCases.AddAsync(violation, cancellationToken);
        }

        edgeNode.MarkHeartbeat(dateTimeProvider.UtcNow, SyncStatus.Online);
        edgeNode.MarkSync(dateTimeProvider.UtcNow, 0);

        return Result<EdgeSyncBatchDto>.Success(batch);
    }
}
