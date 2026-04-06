namespace TPMS.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentCheckoutSession> CreateCheckoutSessionAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken);

    Task<PaymentWebhookPayload> ParseWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken);
}
