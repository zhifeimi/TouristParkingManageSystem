using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TPMS.Edge.Persistence;

namespace TPMS.Edge.Services;

public sealed class MockLprWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<EdgeOptions> options,
    ILogger<MockLprWorker> logger) : BackgroundService
{
    private static readonly string[] DemoPlates = ["ABC123", "PARK24", "EV900", "TRAIL88"];
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!options.Value.EnableMockLpr)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
                var lprService = scope.ServiceProvider.GetRequiredService<EdgeLprService>();

                var bay = await dbContext.Occupancy.OrderBy(entity => entity.BayNumber).FirstOrDefaultAsync(stoppingToken);
                if (bay is not null)
                {
                    await lprService.RecordLprEventAsync(
                        DemoPlates[_random.Next(DemoPlates.Length)],
                        bay.BayId,
                        bay.BayNumber,
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Mock LPR generation failed for the edge node.");
            }

            await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
        }
    }
}
