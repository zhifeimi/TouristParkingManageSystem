using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TPMS.Domain.Aggregates;

namespace TPMS.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RowVersion).IsConcurrencyToken();
        builder.Property(entity => entity.ProviderName).HasMaxLength(50).IsRequired();
        builder.Property(entity => entity.ProviderSessionId).HasMaxLength(200);
        builder.Property(entity => entity.ProviderReference).HasMaxLength(200);
        builder.Property(entity => entity.CheckoutUrl).HasMaxLength(1000);

        builder.OwnsOne(entity => entity.Amount, owned =>
        {
            owned.Property(money => money.Amount)
                .HasColumnName("Amount")
                .HasPrecision(10, 2);

            owned.Property(money => money.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.HasIndex(entity => entity.ProviderSessionId).IsUnique();
        builder.HasIndex(entity => entity.ReservationId).IsUnique();
    }
}
