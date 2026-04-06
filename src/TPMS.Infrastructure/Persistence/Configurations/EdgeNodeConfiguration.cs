using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class EdgeNodeConfiguration : IEntityTypeConfiguration<EdgeNode>
{
    public void Configure(EntityTypeBuilder<EdgeNode> builder)
    {
        builder.ToTable("EdgeNodes");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.NodeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(entity => entity.NodeCode).IsUnique();
    }
}
