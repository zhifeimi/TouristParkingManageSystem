using Microsoft.EntityFrameworkCore;

namespace TPMS.Edge.Persistence;

public sealed class EdgeDbContext(DbContextOptions<EdgeDbContext> options) : DbContext(options)
{
    public DbSet<LocalPermitCacheItem> PermitCache => Set<LocalPermitCacheItem>();

    public DbSet<LocalLprEvent> LprEvents => Set<LocalLprEvent>();

    public DbSet<LocalOccupancyRecord> Occupancy => Set<LocalOccupancyRecord>();

    public DbSet<LocalViolationRecord> Violations => Set<LocalViolationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalPermitCacheItem>(builder =>
        {
            builder.ToTable("PermitCache");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.PermitCode).HasMaxLength(64);
            builder.Property(entity => entity.LicensePlate).HasMaxLength(20);
            builder.Property(entity => entity.BayNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<LocalLprEvent>(builder =>
        {
            builder.ToTable("LprEvents");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.LicensePlate).HasMaxLength(20);
            builder.Property(entity => entity.BayNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<LocalOccupancyRecord>(builder =>
        {
            builder.ToTable("Occupancy");
            builder.HasKey(entity => entity.BayId);
            builder.Property(entity => entity.BayNumber).HasMaxLength(20);
            builder.Property(entity => entity.OccupancyStatus).HasMaxLength(30);
            builder.Property(entity => entity.LicensePlate).HasMaxLength(20);
        });

        modelBuilder.Entity<LocalViolationRecord>(builder =>
        {
            builder.ToTable("Violations");
            builder.HasKey(entity => entity.ViolationCaseId);
            builder.Property(entity => entity.BayNumber).HasMaxLength(20);
            builder.Property(entity => entity.LicensePlate).HasMaxLength(20);
            builder.Property(entity => entity.Reason).HasMaxLength(120);
            builder.Property(entity => entity.Details).HasMaxLength(500);
        });
    }
}
