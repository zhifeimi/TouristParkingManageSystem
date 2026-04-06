using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;

namespace TPMS.Application.Reservations;

internal static class ReservationMappings
{
    public static ReservationDto ToDto(this Reservation reservation, Payment? payment = null)
    {
        var paymentSession = payment?.ProviderSessionId is null || string.IsNullOrWhiteSpace(payment.CheckoutUrl)
            ? null
            : new CreatePaymentSessionResponse(payment.ProviderSessionId, payment.CheckoutUrl);

        return new ReservationDto(
            reservation.Id,
            reservation.ParkingLotId,
            reservation.ParkingBayId,
            reservation.AssignedBayNumber.Value,
            reservation.Status.ToString(),
            reservation.TimeRange.StartUtc,
            reservation.TimeRange.EndUtc,
            reservation.LicensePlate.Value,
            reservation.TotalPrice.Amount,
            reservation.TotalPrice.Currency,
            reservation.Status == ReservationStatus.NeedsResolution,
            reservation.ResolutionNote,
            paymentSession);
    }
}
