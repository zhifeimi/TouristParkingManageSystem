using TPMS.Domain.Aggregates;

namespace TPMS.Application.Abstractions;

public interface IEdgeNodeRepository
{
    Task<EdgeNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
