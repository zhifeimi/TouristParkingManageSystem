using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;
using TPMS.Domain.Enums;

namespace TPMS.Application.Reservations;

public sealed class ReassignBayCommandHandler(
    IReservationRepository reservationRepository,
    IParkingBayRepository parkingBayRepository,
    IParkingLotRepository parkingLotRepository,
    IPaymentRepository paymentRepository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ReassignBayCommand, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(ReassignBayCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation is null)
        {
            return Result<ReservationDto>.NotFound("Reservation was not found.");
        }

        var lot = await parkingLotRepository.GetByIdAsync(reservation.ParkingLotId, cancellationToken);
        if (lot is null)
        {
            return Result<ReservationDto>.NotFound("Parking lot was not found.");
        }

        var currentBay = await parkingBayRepository.GetByIdAsync(reservation.ParkingBayId, cancellationToken);
        if (currentBay is null)
        {
            return Result<ReservationDto>.NotFound("Current parking bay was not found.");
        }

        var targetBay = request.TargetBayId.HasValue
            ? await parkingBayRepository.GetByIdAsync(request.TargetBayId.Value, cancellationToken)
            : await parkingBayRepository.FindFirstAvailableCompatibleBayAsync(
                reservation.ParkingLotId,
                reservation.RequestedBayType,
                reservation.TimeRange,
                reservation.ParkingBayId,
                cancellationToken);

        if (targetBay is null || targetBay.ParkingLotId != reservation.ParkingLotId || targetBay.BayType != reservation.RequestedBayType)
        {
            reservation.MarkNeedsResolution(request.Note ?? "No compatible replacement bay is available.");
            lot.TouchAvailability(dateTimeProvider.UtcNow);
            var unresolvedPayment = await paymentRepository.GetByReservationIdAsync(reservation.Id, cancellationToken);
            return Result<ReservationDto>.Success(reservation.ToDto(unresolvedPayment));
        }

        var hasOverlap = await reservationRepository.HasOverlappingReservationAsync(
            targetBay.Id,
            reservation.TimeRange,
            reservation.Id,
            cancellationToken);

        if (hasOverlap || targetBay.IsUnderMaintenance || !targetBay.IsActive)
        {
            reservation.MarkNeedsResolution(request.Note ?? "The requested replacement bay is unavailable.");
            lot.TouchAvailability(dateTimeProvider.UtcNow);
            var unavailablePayment = await paymentRepository.GetByReservationIdAsync(reservation.Id, cancellationToken);
            return Result<ReservationDto>.Success(reservation.ToDto(unavailablePayment));
        }

        var reason = Enum.TryParse<ReassignmentReason>(request.Reason, true, out var parsedReason)
            ? parsedReason
            : ReassignmentReason.ControllerOverride;

        var now = dateTimeProvider.UtcNow;
        currentBay.TouchAvailability(now);
        targetBay.TouchAvailability(now);
        lot.TouchAvailability(now);
        reservation.Reassign(targetBay.Id, targetBay.BayNumber, reason, now);

        var payment = await paymentRepository.GetByReservationIdAsync(reservation.Id, cancellationToken);
        return Result<ReservationDto>.Success(reservation.ToDto(payment));
    }
}
