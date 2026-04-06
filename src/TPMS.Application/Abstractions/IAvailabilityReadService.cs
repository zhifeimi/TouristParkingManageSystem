using TPMS.Application.Availability;
using TPMS.Application.Lots;
using TPMS.Application.Reservations;

namespace TPMS.Application.Abstractions;

public interface IAvailabilityReadService
{
    Task<IReadOnlyCollection<LotListItemDto>> GetLotsAsync(CancellationToken cancellationToken);

    Task<LotAvailabilitySummaryDto?> GetLotAvailabilityAsync(
        Guid lotId,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken);

    Task<ReservationDto?> GetReservationAsync(Guid reservationId, CancellationToken cancellationToken);
}
