using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPMS.Application.EdgeSync;
using TPMS.Edge.Persistence;

namespace TPMS.Edge.Services;

public sealed class EdgeLprService(EdgeDbContext dbContext, IOptions<EdgeOptions> options)
{
    public async Task<PermitValidationResultDto> ValidatePermitAsync(string licensePlate, CancellationToken cancellationToken)
    {
        var normalizedPlate = new string(licensePlate.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        var now = DateTimeOffset.UtcNow;

        var permitsForPlate = await dbContext.PermitCache
            .AsNoTracking()
            .Where(entity => entity.LicensePlate == normalizedPlate)
            .OrderByDescending(entity => entity.ValidToUtc)
            .ToListAsync(cancellationToken);

        var permit = permitsForPlate.FirstOrDefault(entity =>
            entity.ValidFromUtc <= now &&
            now <= entity.ValidToUtc);

        if (permit is null)
        {
            return new PermitValidationResultDto(
                normalizedPlate,
                false,
                null,
                null,
                null,
                "Missing",
                "No locally cached permit was found.");
        }

        return new PermitValidationResultDto(
            normalizedPlate,
            string.Equals(permit.Status, "Active", StringComparison.OrdinalIgnoreCase),
            permit.PermitCode,
            permit.BayNumber,
            permit.ValidToUtc,
            permit.Status,
            "Permit matched in edge cache.");
    }

    public async Task RecordLprEventAsync(string licensePlate, Guid? bayId, string? bayNumber, CancellationToken cancellationToken)
    {
        var validation = await ValidatePermitAsync(licensePlate, cancellationToken);
        var edgeOptions = options.Value;
        var normalizedPlate = validation.LicensePlate;
        var now = DateTimeOffset.UtcNow;

        await dbContext.LprEvents.AddAsync(new LocalLprEvent
        {
            Id = Guid.NewGuid(),
            LicensePlate = normalizedPlate,
            ParkingLotId = edgeOptions.ParkingLotId,
            ParkingBayId = bayId,
            BayNumber = bayNumber,
            ObservedAtUtc = now,
            PermitMatched = validation.IsValid
        }, cancellationToken);

        if (bayId.HasValue && !string.IsNullOrWhiteSpace(bayNumber))
        {
            var occupancy = await dbContext.Occupancy.SingleOrDefaultAsync(entity => entity.BayId == bayId.Value, cancellationToken);
            if (occupancy is null)
            {
                await dbContext.Occupancy.AddAsync(new LocalOccupancyRecord
                {
                    BayId = bayId.Value,
                    BayNumber = bayNumber,
                    OccupancyStatus = "Occupied",
                    LicensePlate = normalizedPlate,
                    ObservedAtUtc = now
                }, cancellationToken);
            }
            else
            {
                occupancy.OccupancyStatus = "Occupied";
                occupancy.LicensePlate = normalizedPlate;
                occupancy.ObservedAtUtc = now;
                occupancy.SyncedAtUtc = null;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
