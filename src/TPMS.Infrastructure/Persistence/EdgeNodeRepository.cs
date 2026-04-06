using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence;

public sealed class EdgeNodeRepository(TpmsDbContext dbContext) : IEdgeNodeRepository
{
    public Task<EdgeNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.EdgeNodes.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }
}
