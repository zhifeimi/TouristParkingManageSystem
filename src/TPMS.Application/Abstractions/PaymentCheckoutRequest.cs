using TPMS.Domain.ValueObjects;

namespace TPMS.Application.Abstractions;

public sealed record PaymentCheckoutRequest(
    Guid ReservationId,
    string Description,
    Money Amount,
    string SuccessUrl,
    string CancelUrl,
    string CustomerEmail);
