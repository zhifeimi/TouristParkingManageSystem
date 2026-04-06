using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class ParkingLotConfiguration : IEntityTypeConfiguration<ParkingLot>
{
    public void Configure(EntityTypeBuilder<ParkingLot> builder)
    {
        builder.ToTable("ParkingLots");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Code).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();

        builder.OwnsOne(entity => entity.DefaultHourlyRate, owned =>
        {
            owned.Property(money => money.Amount)
                .HasColumnName("DefaultHourlyRateAmount")
                .HasPrecision(10, 2);

            owned.Property(money => money.Currency)
                .HasColumnName("DefaultHourlyRateCurrency")
                .HasMaxLength(3);
        });
    }
}
