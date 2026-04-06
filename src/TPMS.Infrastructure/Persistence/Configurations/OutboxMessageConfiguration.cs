using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EventName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.EventType).HasMaxLength(1000).IsRequired();
        builder.Property(entity => entity.Payload).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.Error).HasMaxLength(2000);
        builder.HasIndex(entity => entity.ProcessedAtUtc);
    }
}
