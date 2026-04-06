namespace TPMS.Application.Reservations;

public sealed record CreatePaymentSessionResponse(string SessionId, string CheckoutUrl);
