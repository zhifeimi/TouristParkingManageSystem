using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence;

public sealed class ParkingBayRepository(TpmsDbContext dbContext) : IParkingBayRepository
{
    public Task<ParkingBay?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ParkingBays.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task AddAsync(ParkingBay parkingBay, CancellationToken cancellationToken)
    {
        return dbContext.ParkingBays.AddAsync(parkingBay, cancellationToken).AsTask();
    }

    public async Task<ParkingBay?> FindFirstAvailableCompatibleBayAsync(
        Guid lotId,
        BayType bayType,
        TimeRange timeRange,
        Guid? excludeBayId,
        CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            ReservationStatus.Held,
            ReservationStatus.Confirmed,
            ReservationStatus.CheckedIn,
            ReservationStatus.NeedsResolution
        };

        return await dbContext.ParkingBays
            .Where(bay =>
                bay.ParkingLotId == lotId &&
                bay.BayType == bayType &&
                bay.IsActive &&
                !bay.IsUnderMaintenance &&
                (!excludeBayId.HasValue || bay.Id != excludeBayId.Value))
            .Where(bay => !dbContext.Reservations.Any(reservation =>
                reservation.ParkingBayId == bay.Id &&
                activeStatuses.Contains(reservation.Status) &&
                reservation.TimeRange.StartUtc < timeRange.EndUtc &&
                timeRange.StartUtc < reservation.TimeRange.EndUtc))
            .OrderBy(bay => bay.BayNumber.Value)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
