using TPMS.Domain.Aggregates;

namespace TPMS.Application.Abstractions;

public interface IPermitRepository
{
    Task AddAsync(Permit permit, CancellationToken cancellationToken);

    Task<Permit?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken);

    Task<Permit?> GetActiveByLicensePlateAsync(Guid parkingLotId, string licensePlate, DateTimeOffset observedAtUtc, CancellationToken cancellationToken);
}
