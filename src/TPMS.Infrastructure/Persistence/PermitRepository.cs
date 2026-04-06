using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;

namespace TPMS.Infrastructure.Persistence;

public sealed class PermitRepository(TpmsDbContext dbContext) : IPermitRepository
{
    public Task AddAsync(Permit permit, CancellationToken cancellationToken)
    {
        return dbContext.Permits.AddAsync(permit, cancellationToken).AsTask();
    }

    public Task<Permit?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return dbContext.Permits.SingleOrDefaultAsync(entity => entity.ReservationId == reservationId, cancellationToken);
    }

    public Task<Permit?> GetActiveByLicensePlateAsync(Guid parkingLotId, string licensePlate, DateTimeOffset observedAtUtc, CancellationToken cancellationToken)
    {
        return dbContext.Permits.SingleOrDefaultAsync(entity =>
            entity.ParkingLotId == parkingLotId &&
            entity.LicensePlate.Value == licensePlate &&
            entity.Status == PermitStatus.Active &&
            entity.ValidFromUtc <= observedAtUtc &&
            observedAtUtc <= entity.ValidToUtc,
            cancellationToken);
    }
}
