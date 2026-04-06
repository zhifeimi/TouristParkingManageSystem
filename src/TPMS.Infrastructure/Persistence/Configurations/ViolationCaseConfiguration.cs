using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class ViolationCaseConfiguration : IEntityTypeConfiguration<ViolationCase>
{
    public void Configure(EntityTypeBuilder<ViolationCase> builder)
    {
        builder.ToTable("ViolationCases");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.Reason).HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Details).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.BayNumber)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : new BayNumber(value))
            .HasMaxLength(20);
        builder.Property(entity => entity.LicensePlate)
            .HasConversion(value => value.Value, value => new LicensePlate(value))
            .HasMaxLength(20);
    }
}
