using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;
using TPMS.Domain.ValueObjects;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.AssignedBayNumber)
            .HasConversion(value => value.Value, value => new BayNumber(value))
            .HasMaxLength(20);
        builder.Property(entity => entity.OriginalBayNumber)
            .HasConversion(value => value.Value, value => new BayNumber(value))
            .HasMaxLength(20);
        builder.Property(entity => entity.LicensePlate)
            .HasConversion(value => value.Value, value => new LicensePlate(value))
            .HasMaxLength(20);
        builder.Property(entity => entity.TouristName).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.TouristEmail).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.ResolutionNote).HasMaxLength(500);

        builder.OwnsOne(entity => entity.TimeRange, owned =>
        {
            owned.Property(value => value.StartUtc).HasColumnName("StartUtc");
            owned.Property(value => value.EndUtc).HasColumnName("EndUtc");
        });

        builder.OwnsOne(entity => entity.TotalPrice, owned =>
        {
            owned.Property(money => money.Amount)
                .HasColumnName("TotalPriceAmount")
                .HasPrecision(10, 2);

            owned.Property(money => money.Currency)
                .HasColumnName("TotalPriceCurrency")
                .HasMaxLength(3);
        });
    }
}
