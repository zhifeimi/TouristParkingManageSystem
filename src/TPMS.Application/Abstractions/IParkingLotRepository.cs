using TPMS.Domain.Aggregates;

namespace TPMS.Application.Abstractions;

public interface IParkingLotRepository
{
    Task<ParkingLot?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(ParkingLot parkingLot, CancellationToken cancellationToken);
}
