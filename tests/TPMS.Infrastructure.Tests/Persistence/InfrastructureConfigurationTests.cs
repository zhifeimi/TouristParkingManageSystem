using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;
using TPMS.Infrastructure.Availability;
using TPMS.Infrastructure.Persistence;

namespace TPMS.Infrastructure.Tests.Persistence;

public sealed class InfrastructureConfigurationTests
{
    [Fact]
    public void RowVersion_should_be_marked_as_concurrency_token()
    {
        var options = new DbContextOptionsBuilder<TpmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new TpmsDbContext(options);

        var property = dbContext.Model.FindEntityType(typeof(ParkingBay))!.FindProperty(nameof(ParkingBay.RowVersion));

        property!.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public async Task AvailabilityReadService_should_project_reserved_bays()
    {
        var options = new DbContextOptionsBuilder<TpmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new TpmsDbContext(options);

        var lot = new ParkingLot(Guid.NewGuid(), "LOT-1", "Main Lot", "Australia/Sydney", new Money(12.5m, "AUD"));
        var bay = new ParkingBay(Guid.NewGuid(), lot.Id, new BayNumber("A1"), BayType.Standard);
        var reservation = Reservation.CreateHold(
            lot.Id,
            bay.Id,
            bay.BayNumber,
            bay.BayType,
            new TimeRange(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(3)),
            new LicensePlate("ABC123"),
            "Guest Tourist",
            "guest@example.com",
            true,
            new Money(25m, "AUD"),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(10));

        await dbContext.ParkingLots.AddAsync(lot);
        await dbContext.ParkingBays.AddAsync(bay);
        await dbContext.Reservations.AddAsync(reservation);
        await dbContext.SaveChangesAsync();

        var service = new AvailabilityReadService(dbContext);
        var summary = await service.GetLotAvailabilityAsync(lot.Id, reservation.TimeRange.StartUtc, reservation.TimeRange.EndUtc, CancellationToken.None);

        summary.Should().NotBeNull();
        summary!.ReservedBays.Should().Be(1);
        summary.AvailableBays.Should().Be(0);
    }
}
