using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Reservations;

public sealed class CreateReservationCommandHandler(
    IParkingLotRepository parkingLotRepository,
    IParkingBayRepository parkingBayRepository,
    IReservationRepository reservationRepository,
    IPaymentRepository paymentRepository,
    IAvailabilityReadService availabilityReadService,
    IPaymentGateway paymentGateway,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

    public async Task<Result<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var lot = await parkingLotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
        {
            return Result<ReservationDto>.NotFound("Parking lot was not found.");
        }

        var bay = await parkingBayRepository.GetByIdAsync(request.BayId, cancellationToken);
        if (bay is null || bay.ParkingLotId != request.LotId)
        {
            return Result<ReservationDto>.NotFound("Parking bay was not found.");
        }

        var timeRange = new TimeRange(request.StartUtc, request.EndUtc);
        var currentAvailability = await availabilityReadService.GetLotAvailabilityAsync(request.LotId, request.StartUtc, request.EndUtc, cancellationToken);

        if (currentAvailability is null)
        {
            return Result<ReservationDto>.NotFound("Parking lot was not found.");
        }

        var selectedBay = currentAvailability.Bays.SingleOrDefault(item => item.BayId == request.BayId);
        if (selectedBay is null)
        {
            return Result<ReservationDto>.NotFound("Parking bay was not found.");
        }

        if (!selectedBay.IsAvailable || bay.IsUnderMaintenance || !bay.IsActive)
        {
            return Result<ReservationDto>.Conflict(
                "The selected bay is no longer available.",
                new { request.LotId, request.BayId, request.StartUtc, request.EndUtc });
        }

        var hasOverlap = await reservationRepository.HasOverlappingReservationAsync(bay.Id, timeRange, null, cancellationToken);
        if (hasOverlap)
        {
            return Result<ReservationDto>.Conflict(
                "The selected bay is already reserved for the requested time window.",
                new { request.LotId, request.BayId, request.StartUtc, request.EndUtc });
        }

        var now = dateTimeProvider.UtcNow;
        var price = lot.CalculateReservationPrice(timeRange);
        var reservation = Reservation.CreateHold(
            request.LotId,
            request.BayId,
            bay.BayNumber,
            bay.BayType,
            timeRange,
            new LicensePlate(request.LicensePlate),
            request.TouristName,
            request.TouristEmail,
            request.IsGuestReservation,
            price,
            now,
            HoldDuration);

        var payment = new Payment(Guid.NewGuid(), reservation.Id, price, "Stripe", now);

        var successUrl = string.IsNullOrWhiteSpace(request.SuccessUrl)
            ? "https://localhost:5173/tourist?payment=success"
            : request.SuccessUrl!;

        var cancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl)
            ? "https://localhost:5173/tourist?payment=cancelled"
            : request.CancelUrl!;

        var checkoutSession = await paymentGateway.CreateCheckoutSessionAsync(
            new PaymentCheckoutRequest(
                reservation.Id,
                $"TPMS reservation {reservation.AssignedBayNumber.Value}",
                price,
                successUrl,
                cancelUrl,
                request.TouristEmail),
            cancellationToken);

        payment.AttachCheckoutSession(checkoutSession.SessionId, checkoutSession.CheckoutUrl, now);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await paymentRepository.AddAsync(payment, cancellationToken);

        bay.TouchAvailability(now);
        lot.TouchAvailability(now);

        return Result<ReservationDto>.Success(reservation.ToDto(payment));
    }
}
