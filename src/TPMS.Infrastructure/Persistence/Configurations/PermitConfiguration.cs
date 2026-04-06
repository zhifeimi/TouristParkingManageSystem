using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class PermitConfiguration : IEntityTypeConfiguration<Permit>
{
    public void Configure(EntityTypeBuilder<Permit> builder)
    {
        builder.ToTable("Permits");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.PermitCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.BayNumber)
            .HasConversion(value => value.Value, value => new BayNumber(value))
            .HasMaxLength(20);
        builder.Property(entity => entity.LicensePlate)
            .HasConversion(value => value.Value, value => new LicensePlate(value))
            .HasMaxLength(20);
        builder.HasIndex(entity => entity.ReservationId).IsUnique();
    }
}
