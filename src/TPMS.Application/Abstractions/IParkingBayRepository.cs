using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Abstractions;

public interface IParkingBayRepository
{
    Task<ParkingBay?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(ParkingBay parkingBay, CancellationToken cancellationToken);

    Task<ParkingBay?> FindFirstAvailableCompatibleBayAsync(
        Guid lotId,
        BayType bayType,
        TimeRange timeRange,
        Guid? excludeBayId,
        CancellationToken cancellationToken);
}
