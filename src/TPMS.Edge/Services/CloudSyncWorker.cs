using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPMS.Application.EdgeSync;
using TPMS.Edge.Persistence;

namespace TPMS.Edge.Services;

public sealed class CloudSyncWorker(
    IServiceScopeFactory serviceScopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<EdgeOptions> options,
    ILogger<CloudSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var edgeOptions = options.Value;
                if (edgeOptions.EdgeNodeId == Guid.Empty || string.IsNullOrWhiteSpace(edgeOptions.CloudBaseUrl))
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                var httpClient = httpClientFactory.CreateClient("cloud");
                httpClient.BaseAddress = new Uri(edgeOptions.CloudBaseUrl);

                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

                var pullBatch = await httpClient.GetFromJsonAsync<EdgeSyncBatchDto>(
                    $"/api/edge/sync/pull?edgeNodeId={edgeOptions.EdgeNodeId}",
                    stoppingToken);

                if (pullBatch is not null)
                {
                    await ApplyPullBatchAsync(dbContext, pullBatch, stoppingToken);
                }

                var pushBatch = await BuildPushBatchAsync(dbContext, edgeOptions.EdgeNodeId, stoppingToken);
                var pushResponse = await httpClient.PostAsJsonAsync(
                    $"/api/edge/sync/push?edgeNodeId={edgeOptions.EdgeNodeId}",
                    pushBatch,
                    stoppingToken);

                if (pushResponse.IsSuccessStatusCode)
                {
                    await MarkSyncedAsync(dbContext, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Edge sync iteration failed. The node will retry on the next cycle.");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private static async Task ApplyPullBatchAsync(EdgeDbContext dbContext, EdgeSyncBatchDto batch, CancellationToken cancellationToken)
    {
        foreach (var permit in batch.Permits)
        {
            var existing = await dbContext.PermitCache.SingleOrDefaultAsync(entity => entity.Id == permit.PermitId, cancellationToken);
            if (existing is null)
            {
                await dbContext.PermitCache.AddAsync(new LocalPermitCacheItem
                {
                    Id = permit.PermitId,
                    ReservationId = permit.ReservationId,
                    PermitCode = permit.PermitCode,
                    LicensePlate = permit.LicensePlate,
                    BayNumber = permit.BayNumber,
                    ValidFromUtc = permit.ValidFromUtc,
                    ValidToUtc = permit.ValidToUtc,
                    Status = permit.Status,
                    SyncedAtUtc = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
            else
            {
                existing.PermitCode = permit.PermitCode;
                existing.LicensePlate = permit.LicensePlate;
                existing.BayNumber = permit.BayNumber;
                existing.ValidFromUtc = permit.ValidFromUtc;
                existing.ValidToUtc = permit.ValidToUtc;
                existing.Status = permit.Status;
                existing.SyncedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        foreach (var occupancyChange in batch.OccupancyChanges)
        {
            var existing = await dbContext.Occupancy.SingleOrDefaultAsync(entity => entity.BayId == occupancyChange.BayId, cancellationToken);
            if (existing is null)
            {
                await dbContext.Occupancy.AddAsync(new LocalOccupancyRecord
                {
                    BayId = occupancyChange.BayId,
                    BayNumber = occupancyChange.BayNumber,
                    OccupancyStatus = occupancyChange.OccupancyStatus,
                    LicensePlate = occupancyChange.LicensePlate,
                    ObservedAtUtc = occupancyChange.ObservedAtUtc,
                    SyncedAtUtc = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
            else
            {
                existing.BayNumber = occupancyChange.BayNumber;
                existing.OccupancyStatus = occupancyChange.OccupancyStatus;
                existing.LicensePlate = occupancyChange.LicensePlate;
                existing.ObservedAtUtc = occupancyChange.ObservedAtUtc;
                existing.SyncedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<EdgeSyncBatchDto> BuildPushBatchAsync(EdgeDbContext dbContext, Guid edgeNodeId, CancellationToken cancellationToken)
    {
        var occupancyChanges = await dbContext.Occupancy
            .AsNoTracking()
            .Where(entity => entity.SyncedAtUtc == null)
            .Select(entity => new EdgeOccupancyChangeDto(
                entity.BayId,
                entity.BayNumber,
                entity.OccupancyStatus,
                entity.LicensePlate,
                entity.ObservedAtUtc))
            .ToListAsync(cancellationToken);

        var lprEvents = await dbContext.LprEvents
            .AsNoTracking()
            .Where(entity => entity.SyncedAtUtc == null)
            .Select(entity => new EdgeLprEventDto(
                entity.Id,
                entity.LicensePlate,
                entity.ParkingLotId,
                entity.ParkingBayId,
                entity.BayNumber,
                entity.ObservedAtUtc,
                entity.PermitMatched))
            .ToListAsync(cancellationToken);

        var violations = await dbContext.Violations
            .AsNoTracking()
            .Where(entity => entity.SyncedAtUtc == null)
            .Select(entity => new EdgeViolationRecordDto(
                entity.ViolationCaseId,
                entity.ParkingLotId,
                entity.ParkingBayId,
                entity.BayNumber,
                entity.LicensePlate,
                entity.Reason,
                entity.Details,
                entity.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new EdgeSyncBatchDto(
            edgeNodeId,
            DateTimeOffset.UtcNow,
            [],
            occupancyChanges,
            lprEvents,
            violations);
    }

    private static async Task MarkSyncedAsync(EdgeDbContext dbContext, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        await dbContext.LprEvents
            .Where(entity => entity.SyncedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.SyncedAtUtc, now), cancellationToken);

        await dbContext.Occupancy
            .Where(entity => entity.SyncedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.SyncedAtUtc, now), cancellationToken);

        await dbContext.Violations
            .Where(entity => entity.SyncedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.SyncedAtUtc, now), cancellationToken);
    }
}
