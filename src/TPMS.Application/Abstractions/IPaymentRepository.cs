using TPMS.Domain.Aggregates;

namespace TPMS.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    Task<Payment?> GetByProviderSessionIdAsync(string providerSessionId, CancellationToken cancellationToken);

    Task<Payment?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken);
}
