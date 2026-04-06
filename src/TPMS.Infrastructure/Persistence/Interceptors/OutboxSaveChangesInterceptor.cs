using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TPMS.Domain.Common;

namespace TPMS.Infrastructure.Persistence.Interceptors;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureDomainEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureDomainEvents(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void CaptureDomainEvents(DbContext? context)
    {
        if (context is not TpmsDbContext dbContext)
        {
            return;
        }

        var domainEntities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        if (domainEntities.Count == 0)
        {
            return;
        }

        var outboxMessages = new List<OutboxMessage>();

        foreach (var entity in domainEntities)
        {
            outboxMessages.AddRange(entity.DomainEvents.Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventName = domainEvent.GetType().Name,
                EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                CreatedAtUtc = domainEvent.OccurredOnUtc
            }));

            entity.ClearDomainEvents();
        }

        dbContext.OutboxMessages.AddRange(outboxMessages);
    }
}
