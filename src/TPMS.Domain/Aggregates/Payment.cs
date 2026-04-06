using TPMS.Domain.Common;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class Payment : AggregateRoot<Guid>
{
    private Payment()
    {
    }

    public Payment(Guid id, Guid reservationId, Money amount, string providerName, DateTimeOffset createdAtUtc)
        : base(id)
    {
        ReservationId = reservationId;
        Amount = amount;
        ProviderName = providerName.Trim();
        Status = PaymentStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid ReservationId { get; private set; }

    public Money Amount { get; private set; } = Money.Zero();

    public string ProviderName { get; private set; } = string.Empty;

    public string? ProviderSessionId { get; private set; }

    public string? ProviderReference { get; private set; }

    public string? CheckoutUrl { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void AttachCheckoutSession(string providerSessionId, string checkoutUrl, DateTimeOffset nowUtc)
    {
        ProviderSessionId = providerSessionId;
        CheckoutUrl = checkoutUrl;
        Status = PaymentStatus.CheckoutCreated;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkSucceeded(string providerReference, DateTimeOffset nowUtc)
    {
        ProviderReference = providerReference;
        Status = PaymentStatus.Succeeded;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkFailed(string? providerReference, DateTimeOffset nowUtc)
    {
        ProviderReference = providerReference;
        Status = PaymentStatus.Failed;
        UpdatedAtUtc = nowUtc;
    }
}
