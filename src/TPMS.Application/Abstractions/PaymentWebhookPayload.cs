namespace TPMS.Application.Abstractions;

public sealed record PaymentWebhookPayload(string SessionId, bool IsSuccess, string ProviderReference);
