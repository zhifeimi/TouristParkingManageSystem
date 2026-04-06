using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace TPMS.Edge.Persistence;

public sealed class EdgeSeeder(EdgeDbContext dbContext, IOptions<EdgeOptions> options)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var edgeOptions = options.Value;
        if (edgeOptions.EdgeNodeId == Guid.Empty || edgeOptions.ParkingLotId == Guid.Empty)
        {
            return;
        }

        if (!await dbContext.Occupancy.AnyAsync(cancellationToken))
        {
            await dbContext.Occupancy.AddRangeAsync(
            [
                new LocalOccupancyRecord
                {
                    BayId = Guid.NewGuid(),
                    BayNumber = "A1",
                    OccupancyStatus = "Vacant",
                    ObservedAtUtc = DateTimeOffset.UtcNow
                },
                new LocalOccupancyRecord
                {
                    BayId = Guid.NewGuid(),
                    BayNumber = "A2",
                    OccupancyStatus = "Vacant",
                    ObservedAtUtc = DateTimeOffset.UtcNow
                }
            ], cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
