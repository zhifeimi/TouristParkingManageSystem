using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class ParkingBayConfiguration : IEntityTypeConfiguration<ParkingBay>
{
    public void Configure(EntityTypeBuilder<ParkingBay> builder)
    {
        builder.ToTable("ParkingBays");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.BayNumber)
            .HasConversion(value => value.Value, value => new BayNumber(value))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(entity => entity.OccupiedByLicensePlate).HasMaxLength(20);
        builder.HasIndex(entity => new { entity.ParkingLotId, entity.BayNumber }).IsUnique();
    }
}
