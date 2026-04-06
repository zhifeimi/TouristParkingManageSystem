using MediatR;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;
using TPMS.Domain.Aggregates;

namespace TPMS.Application.Reservations;

public sealed class HandleStripeWebhookCommandHandler(
    IPaymentGateway paymentGateway,
    IPaymentRepository paymentRepository,
    IReservationRepository reservationRepository,
    IPermitRepository permitRepository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<HandleStripeWebhookCommand, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhook = await paymentGateway.ParseWebhookAsync(request.Payload, request.SignatureHeader, cancellationToken);
        var payment = await paymentRepository.GetByProviderSessionIdAsync(webhook.SessionId, cancellationToken);

        if (payment is null)
        {
            return Result<ReservationDto>.NotFound("Payment session was not found.");
        }

        var reservation = await reservationRepository.GetByIdAsync(payment.ReservationId, cancellationToken);
        if (reservation is null)
        {
            return Result<ReservationDto>.NotFound("Reservation was not found.");
        }

        if (webhook.IsSuccess)
        {
            payment.MarkSucceeded(webhook.ProviderReference, dateTimeProvider.UtcNow);
            if (reservation.Status != Domain.Enums.ReservationStatus.Confirmed)
            {
                reservation.ConfirmPayment(dateTimeProvider.UtcNow);
            }

            var existingPermit = await permitRepository.GetByReservationIdAsync(reservation.Id, cancellationToken);
            if (existingPermit is null)
            {
                await permitRepository.AddAsync(Permit.IssueFromReservation(reservation), cancellationToken);
            }
        }
        else
        {
            payment.MarkFailed(webhook.ProviderReference, dateTimeProvider.UtcNow);
            reservation.MarkNeedsResolution("Payment failed and requires follow-up.");
        }

        return Result<ReservationDto>.Success(reservation.ToDto(payment));
    }
}
