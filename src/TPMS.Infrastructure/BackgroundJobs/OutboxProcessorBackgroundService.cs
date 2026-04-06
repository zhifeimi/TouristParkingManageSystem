using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TPMS.Application.Abstractions;
using TPMS.Application.Availability;
using TPMS.Application.Enforcement;
using TPMS.Domain.Events;
using TPMS.Infrastructure.Persistence;

namespace TPMS.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessorBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TpmsDbContext>();
                var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
                var availabilityReadService = scope.ServiceProvider.GetRequiredService<IAvailabilityReadService>();

                var messages = await dbContext.OutboxMessages
                    .Where(message => message.ProcessedAtUtc == null)
                    .OrderBy(message => message.CreatedAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        switch (message.EventName)
                        {
                            case nameof(ReservationHeldDomainEvent):
                            {
                                var domainEvent = JsonSerializer.Deserialize<ReservationHeldDomainEvent>(message.Payload)!;
                                var reservation = await availabilityReadService.GetReservationAsync(domainEvent.ReservationId, stoppingToken);
                                var availability = await availabilityReadService.GetLotAvailabilityAsync(
                                    domainEvent.ParkingLotId,
                                    domainEvent.StartUtc,
                                    domainEvent.EndUtc,
                                    stoppingToken);

                                if (reservation is not null)
                                {
                                    await notifier.PublishReservationChangedAsync(reservation, stoppingToken);
                                }

                                if (availability is not null)
                                {
                                    await notifier.PublishLotAvailabilityUpdatedAsync(availability, stoppingToken);
                                }

                                break;
                            }

                            case nameof(ReservationConfirmedDomainEvent):
                            {
                                var domainEvent = JsonSerializer.Deserialize<ReservationConfirmedDomainEvent>(message.Payload)!;
                                var reservation = await availabilityReadService.GetReservationAsync(domainEvent.ReservationId, stoppingToken);
                                if (reservation is not null)
                                {
                                    await notifier.PublishReservationChangedAsync(reservation, stoppingToken);

                                    var availability = await availabilityReadService.GetLotAvailabilityAsync(
                                        domainEvent.ParkingLotId,
                                        reservation.StartUtc,
                                        reservation.EndUtc,
                                        stoppingToken);

                                    if (availability is not null)
                                    {
                                        await notifier.PublishLotAvailabilityUpdatedAsync(availability, stoppingToken);
                                    }
                                }

                                break;
                            }

                            case nameof(ReservationReassignedDomainEvent):
                            {
                                var domainEvent = JsonSerializer.Deserialize<ReservationReassignedDomainEvent>(message.Payload)!;
                                var reservation = await availabilityReadService.GetReservationAsync(domainEvent.ReservationId, stoppingToken);
                                if (reservation is not null)
                                {
                                    await notifier.PublishReservationChangedAsync(reservation, stoppingToken);
                                    var availability = await availabilityReadService.GetLotAvailabilityAsync(
                                        domainEvent.ParkingLotId,
                                        reservation.StartUtc,
                                        reservation.EndUtc,
                                        stoppingToken);

                                    if (availability is not null)
                                    {
                                        await notifier.PublishLotAvailabilityUpdatedAsync(availability, stoppingToken);
                                    }
                                }

                                break;
                            }

                            case nameof(BayOccupancyChangedDomainEvent):
                            {
                                var domainEvent = JsonSerializer.Deserialize<BayOccupancyChangedDomainEvent>(message.Payload)!;
                                await notifier.PublishBayOccupancyUpdatedAsync(
                                    new BayOccupancySignal(
                                        domainEvent.ParkingLotId,
                                        domainEvent.ParkingBayId,
                                        domainEvent.BayNumber,
                                        domainEvent.OccupancyStatus.ToString(),
                                        null),
                                    stoppingToken);
                                break;
                            }

                            case nameof(ViolationRaisedDomainEvent):
                            {
                                var domainEvent = JsonSerializer.Deserialize<ViolationRaisedDomainEvent>(message.Payload)!;
                                await notifier.PublishViolationRaisedAsync(
                                    new ViolationDto(
                                        domainEvent.ViolationCaseId,
                                        domainEvent.ParkingLotId,
                                        domainEvent.ParkingBayId,
                                        null,
                                        domainEvent.LicensePlate,
                                        domainEvent.Reason,
                                        string.Empty,
                                        "Open",
                                        domainEvent.OccurredOnUtc),
                                    stoppingToken);
                                break;
                            }
                        }

                        message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                        message.Error = null;
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;
                        logger.LogError(ex, "Failed to process TPMS outbox event {EventName}.", message.EventName);
                    }
                }

                if (messages.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TPMS outbox processor iteration failed.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
