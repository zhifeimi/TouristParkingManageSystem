using Microsoft.EntityFrameworkCore;
using TPMS.Application.Abstractions;
using TPMS.Application.Common;

namespace TPMS.Infrastructure.Persistence;

public sealed class EfUnitOfWork(TpmsDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The reservation data changed before your update could be saved. Please refresh availability and try again.")
            {
                Source = ex.Source
            };
        }
    }
}
