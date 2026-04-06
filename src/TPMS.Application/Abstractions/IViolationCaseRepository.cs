using TPMS.Domain.Aggregates;

namespace TPMS.Application.Abstractions;

public interface IViolationCaseRepository
{
    Task AddAsync(ViolationCase violationCase, CancellationToken cancellationToken);
}
