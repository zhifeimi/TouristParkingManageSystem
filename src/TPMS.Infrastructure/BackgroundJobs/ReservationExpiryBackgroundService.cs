using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TPMS.Application.Abstractions;

namespace TPMS.Infrastructure.BackgroundJobs;

public sealed class ReservationExpiryBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ReservationExpiryBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var reservations = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiredReservations = await reservations.ListExpiredHeldReservationsAsync(clock.UtcNow, stoppingToken);
                foreach (var reservation in expiredReservations)
                {
                    reservation.ExpireHold(clock.UtcNow);
                }

                if (expiredReservations.Count > 0)
                {
                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed while expiring TPMS reservation holds.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
