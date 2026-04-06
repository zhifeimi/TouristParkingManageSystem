using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Abstractions;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken);

    Task<bool> HasOverlappingReservationAsync(
        Guid parkingBayId,
        TimeRange timeRange,
        Guid? excludeReservationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Reservation>> ListExpiredHeldReservationsAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken);
}
