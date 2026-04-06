using Microsoft.AspNetCore.SignalR;
using TPMS.Api.Hubs;
using TPMS.Application.Abstractions;
using TPMS.Application.Availability;
using TPMS.Application.Enforcement;
using TPMS.Application.Reservations;

namespace TPMS.Api.Realtime;

public sealed class SignalRRealtimeNotifier(IHubContext<OccupancyHub> hubContext) : IRealtimeNotifier
{
    public Task PublishBayOccupancyUpdatedAsync(BayOccupancySignal payload, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("BayOccupancyUpdated", payload, cancellationToken);
    }

    public Task PublishLotAvailabilityUpdatedAsync(LotAvailabilitySummaryDto payload, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("LotAvailabilityUpdated", payload, cancellationToken);
    }

    public Task PublishReservationChangedAsync(ReservationDto payload, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("ReservationChanged", payload, cancellationToken);
    }

    public Task PublishViolationRaisedAsync(ViolationDto payload, CancellationToken cancellationToken)
    {
        return hubContext.Clients.All.SendAsync("ViolationRaised", payload, cancellationToken);
    }
}
