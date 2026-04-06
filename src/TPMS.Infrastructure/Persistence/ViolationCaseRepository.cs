using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence;

public sealed class ViolationCaseRepository(TpmsDbContext dbContext) : IViolationCaseRepository
{
    public Task AddAsync(ViolationCase violationCase, CancellationToken cancellationToken)
    {
        return dbContext.ViolationCases.AddAsync(violationCase, cancellationToken).AsTask();
    }
}
