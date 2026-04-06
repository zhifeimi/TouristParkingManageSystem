using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence;

public sealed class ParkingLotRepository(TpmsDbContext dbContext) : IParkingLotRepository
{
    public Task<ParkingLot?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ParkingLots.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task AddAsync(ParkingLot parkingLot, CancellationToken cancellationToken)
    {
        return dbContext.ParkingLots.AddAsync(parkingLot, cancellationToken).AsTask();
    }
}
