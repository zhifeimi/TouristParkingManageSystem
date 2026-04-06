using TPMS.Application.Availability;
using TPMS.Application.Enforcement;
using TPMS.Application.Reservations;

namespace TPMS.Application.Abstractions;

public interface IRealtimeNotifier
{
    Task PublishBayOccupancyUpdatedAsync(BayOccupancySignal payload, CancellationToken cancellationToken);

    Task PublishLotAvailabilityUpdatedAsync(LotAvailabilitySummaryDto payload, CancellationToken cancellationToken);

    Task PublishReservationChangedAsync(ReservationDto payload, CancellationToken cancellationToken);

    Task PublishViolationRaisedAsync(ViolationDto payload, CancellationToken cancellationToken);
}
