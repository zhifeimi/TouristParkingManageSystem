using System.Text.Json;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TPMS.Application.Abstractions;

namespace TPMS.Infrastructure.Payments;

public sealed class StripePaymentGateway(IOptions<StripeOptions> options) : IPaymentGateway
{
    public async Task<PaymentCheckoutSession> CreateCheckoutSessionAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken)
    {
        var stripeOptions = options.Value;

        if (string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
        {
            var fakeSessionId = $"dev_{request.ReservationId:N}";
            return new PaymentCheckoutSession(fakeSessionId, $"{request.SuccessUrl}?session_id={fakeSessionId}");
        }

        StripeConfiguration.ApiKey = stripeOptions.SecretKey;

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(
            new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                CustomerEmail = request.CustomerEmail,
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Amount.Currency.ToLowerInvariant(),
                            UnitAmountDecimal = request.Amount.Amount * 100m,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.Description
                            }
                        }
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["reservationId"] = request.ReservationId.ToString()
                }
            },
            cancellationToken: cancellationToken);

        return new PaymentCheckoutSession(session.Id, session.Url ?? request.SuccessUrl);
    }

    public Task<PaymentWebhookPayload> ParseWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var stripeOptions = options.Value;
        if (string.IsNullOrWhiteSpace(stripeOptions.WebhookSecret))
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var sessionId = root.TryGetProperty("sessionId", out var sessionIdProperty)
                ? sessionIdProperty.GetString() ?? string.Empty
                : string.Empty;

            var isSuccess = root.TryGetProperty("isSuccess", out var successProperty) && successProperty.GetBoolean();
            var reference = root.TryGetProperty("providerReference", out var referenceProperty)
                ? referenceProperty.GetString() ?? sessionId
                : sessionId;

            return Task.FromResult(new PaymentWebhookPayload(sessionId, isSuccess, reference));
        }

        var stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, stripeOptions.WebhookSecret);
        if (stripeEvent.Data.Object is not Session session)
        {
            throw new InvalidOperationException("Stripe webhook did not contain a checkout session.");
        }

        var isSuccessful = stripeEvent.Type == EventTypes.CheckoutSessionCompleted;
        return Task.FromResult(new PaymentWebhookPayload(session.Id, isSuccessful, session.PaymentIntentId ?? session.Id));
    }
}
