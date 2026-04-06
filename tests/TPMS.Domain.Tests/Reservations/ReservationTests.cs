using FluentAssertions;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Tests.Reservations;

public sealed class ReservationTests
{
    [Fact]
    public void CreateHold_should_expire_after_hold_window()
    {
        var reservation = Reservation.CreateHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BayNumber("A1"),
            BayType.Standard,
            new TimeRange(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(3)),
            new LicensePlate("abc123"),
            "Guest Tourist",
            "guest@example.com",
            true,
            new Money(25m, "AUD"),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(10));

        reservation.ExpireHold(DateTimeOffset.UtcNow.AddMinutes(11));

        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Reassign_should_update_bay_and_reason()
    {
        var reservation = Reservation.CreateHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BayNumber("A1"),
            BayType.Standard,
            new TimeRange(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(3)),
            new LicensePlate("abc123"),
            "Guest Tourist",
            "guest@example.com",
            true,
            new Money(25m, "AUD"),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(10));

        var newBayId = Guid.NewGuid();

        reservation.Reassign(newBayId, new BayNumber("A3"), ReassignmentReason.ControllerOverride, DateTimeOffset.UtcNow);

        reservation.ParkingBayId.Should().Be(newBayId);
        reservation.AssignedBayNumber.Value.Should().Be("A3");
        reservation.LastReassignmentReason.Should().Be(ReassignmentReason.ControllerOverride);
    }

    [Fact]
    public void Issued_permit_should_validate_matching_plate()
    {
        var reservation = Reservation.CreateHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BayNumber("C2"),
            BayType.EV,
            new TimeRange(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2)),
            new LicensePlate("ev900"),
            "EV Driver",
            "ev@example.com",
            false,
            new Money(15m, "AUD"),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(10));

        reservation.ConfirmPayment(DateTimeOffset.UtcNow);
        var permit = Permit.IssueFromReservation(reservation);

        permit.IsValidFor(new LicensePlate("EV900"), DateTimeOffset.UtcNow.AddHours(1.5)).Should().BeTrue();
    }
}
