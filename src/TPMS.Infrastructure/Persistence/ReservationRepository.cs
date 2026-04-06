using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence;

public sealed class ReservationRepository(TpmsDbContext dbContext) : IReservationRepository
{
    private static readonly ReservationStatus[] ActiveStatuses =
    [
        ReservationStatus.Held,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
        ReservationStatus.NeedsResolution
    ];

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Reservations.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        return dbContext.Reservations.AddAsync(reservation, cancellationToken).AsTask();
    }

    public Task<bool> HasOverlappingReservationAsync(
        Guid parkingBayId,
        TimeRange timeRange,
        Guid? excludeReservationId,
        CancellationToken cancellationToken)
    {
        return dbContext.Reservations.AnyAsync(reservation =>
            reservation.ParkingBayId == parkingBayId &&
            (!excludeReservationId.HasValue || reservation.Id != excludeReservationId.Value) &&
            ActiveStatuses.Contains(reservation.Status) &&
            reservation.TimeRange.StartUtc < timeRange.EndUtc &&
            timeRange.StartUtc < reservation.TimeRange.EndUtc,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reservation>> ListExpiredHeldReservationsAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.Held && reservation.HoldExpiresAtUtc <= nowUtc)
            .ToListAsync(cancellationToken);
    }
}
