using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence;

public sealed class PaymentRepository(TpmsDbContext dbContext) : IPaymentRepository
{
    public Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        return dbContext.Payments.AddAsync(payment, cancellationToken).AsTask();
    }

    public Task<Payment?> GetByProviderSessionIdAsync(string providerSessionId, CancellationToken cancellationToken)
    {
        return dbContext.Payments.SingleOrDefaultAsync(entity => entity.ProviderSessionId == providerSessionId, cancellationToken);
    }

    public Task<Payment?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return dbContext.Payments.SingleOrDefaultAsync(entity => entity.ReservationId == reservationId, cancellationToken);
    }
}
