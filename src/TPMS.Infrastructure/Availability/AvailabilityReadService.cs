using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Application.Availability;
using TPMS.Application.Lots;
using TPMS.Application.Reservations;
using TPMS.Domain.Enums;
using TPMS.Infrastructure.Persistence;

namespace TPMS.Infrastructure.Availability;

public sealed class AvailabilityReadService(TpmsDbContext dbContext) : IAvailabilityReadService
{
    private static readonly ReservationStatus[] ActiveStatuses =
    [
        ReservationStatus.Held,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
        ReservationStatus.NeedsResolution
    ];

    public async Task<IReadOnlyCollection<LotListItemDto>> GetLotsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ParkingLots
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .Select(entity => new LotListItemDto(
                entity.Id,
                entity.Code,
                entity.Name,
                entity.TimeZoneId,
                entity.DefaultHourlyRate.Amount,
                entity.DefaultHourlyRate.Currency))
            .ToListAsync(cancellationToken);
    }

    public async Task<LotAvailabilitySummaryDto?> GetLotAvailabilityAsync(
        Guid lotId,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        var lot = await dbContext.ParkingLots
            .AsNoTracking()
            .Where(entity => entity.Id == lotId)
            .Select(entity => new { entity.Id, entity.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (lot is null)
        {
            return null;
        }

        var reservedBayIds = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.ParkingLotId == lotId &&
                ActiveStatuses.Contains(reservation.Status) &&
                reservation.TimeRange.StartUtc < endUtc &&
                startUtc < reservation.TimeRange.EndUtc)
            .Select(reservation => reservation.ParkingBayId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var bays = await dbContext.ParkingBays
            .AsNoTracking()
            .Where(entity => entity.ParkingLotId == lotId)
            .OrderBy(entity => entity.BayNumber.Value)
            .Select(entity => new BayAvailabilityDto(
                entity.Id,
                entity.BayNumber.Value,
                entity.BayType.ToString(),
                entity.IsActive && !entity.IsUnderMaintenance && entity.OccupancyStatus != OccupancyStatus.Occupied && !reservedBayIds.Contains(entity.Id),
                reservedBayIds.Contains(entity.Id),
                entity.OccupancyStatus == OccupancyStatus.Occupied,
                entity.IsUnderMaintenance,
                entity.OccupiedByLicensePlate))
            .ToListAsync(cancellationToken);

        return new LotAvailabilitySummaryDto(
            lot.Id,
            lot.Name,
            startUtc,
            endUtc,
            bays.Count,
            bays.Count(item => item.IsAvailable),
            bays.Count(item => item.IsOccupied),
            bays.Count(item => item.IsReserved),
            bays);
    }

    public async Task<ReservationDto?> GetReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Where(entity => entity.Id == reservationId)
            .Select(entity => new ReservationDto(
                entity.Id,
                entity.ParkingLotId,
                entity.ParkingBayId,
                entity.AssignedBayNumber.Value,
                entity.Status.ToString(),
                entity.TimeRange.StartUtc,
                entity.TimeRange.EndUtc,
                entity.LicensePlate.Value,
                entity.TotalPrice.Amount,
                entity.TotalPrice.Currency,
                entity.Status == ReservationStatus.NeedsResolution,
                entity.ResolutionNote,
                dbContext.Payments
                    .Where(payment => payment.ReservationId == entity.Id && payment.ProviderSessionId != null && payment.CheckoutUrl != null)
                    .Select(payment => new CreatePaymentSessionResponse(payment.ProviderSessionId!, payment.CheckoutUrl!))
                    .FirstOrDefault()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
