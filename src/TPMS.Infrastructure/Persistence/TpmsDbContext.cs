using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TPMS.Domain.Aggregates;
using TPMS.Infrastructure.Auth;

namespace TPMS.Infrastructure.Persistence;

public sealed class TpmsDbContext(DbContextOptions<TpmsDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();

    public DbSet<ParkingBay> ParkingBays => Set<ParkingBay>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<Permit> Permits => Set<Permit>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<ViolationCase> ViolationCases => Set<ViolationCase>();

    public DbSet<EdgeNode> EdgeNodes => Set<EdgeNode>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TpmsDbContext).Assembly);
    }
}
