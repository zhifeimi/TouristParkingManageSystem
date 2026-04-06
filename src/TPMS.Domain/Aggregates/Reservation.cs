using TPMS.Domain.Common;
using TPMS.Domain.Enums;
using TPMS.Domain.Events;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class Reservation : AggregateRoot<Guid>
{
    private Reservation()
    {
    }

    private Reservation(
        Guid id,
        Guid parkingLotId,
        Guid parkingBayId,
        BayNumber selectedBayNumber,
        BayType requestedBayType,
        TimeRange timeRange,
        LicensePlate licensePlate,
        string touristName,
        string touristEmail,
        bool isGuestReservation,
        Money totalPrice,
        DateTimeOffset createdAtUtc,
        DateTimeOffset holdExpiresAtUtc)
        : base(id)
    {
        ParkingLotId = parkingLotId;
        ParkingBayId = parkingBayId;
        OriginalParkingBayId = parkingBayId;
        AssignedBayNumber = selectedBayNumber;
        OriginalBayNumber = selectedBayNumber;
        RequestedBayType = requestedBayType;
        TimeRange = timeRange;
        LicensePlate = licensePlate;
        TouristName = touristName.Trim();
        TouristEmail = touristEmail.Trim();
        IsGuestReservation = isGuestReservation;
        TotalPrice = totalPrice;
        Status = ReservationStatus.Held;
        CreatedAtUtc = createdAtUtc;
        HoldExpiresAtUtc = holdExpiresAtUtc;
    }

    public Guid ParkingLotId { get; private set; }

    public Guid ParkingBayId { get; private set; }

    public Guid OriginalParkingBayId { get; private set; }

    public BayNumber AssignedBayNumber { get; private set; } = new("UNASSIGNED");

    public BayNumber OriginalBayNumber { get; private set; } = new("UNASSIGNED");

    public BayType RequestedBayType { get; private set; }

    public TimeRange TimeRange { get; private set; } = new(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

    public LicensePlate LicensePlate { get; private set; } = new("UNKNOWN");

    public string TouristName { get; private set; } = string.Empty;

    public string TouristEmail { get; private set; } = string.Empty;

    public bool IsGuestReservation { get; private set; }

    public Money TotalPrice { get; private set; } = Money.Zero();

    public ReservationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset HoldExpiresAtUtc { get; private set; }

    public DateTimeOffset? ConfirmedAtUtc { get; private set; }

    public DateTimeOffset? CheckedInAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? ResolutionNote { get; private set; }

    public ReassignmentReason? LastReassignmentReason { get; private set; }

    public static Reservation CreateHold(
        Guid parkingLotId,
        Guid parkingBayId,
        BayNumber selectedBayNumber,
        BayType requestedBayType,
        TimeRange timeRange,
        LicensePlate licensePlate,
        string touristName,
        string touristEmail,
        bool isGuestReservation,
        Money totalPrice,
        DateTimeOffset nowUtc,
        TimeSpan holdDuration)
    {
        var reservation = new Reservation(
            Guid.NewGuid(),
            parkingLotId,
            parkingBayId,
            selectedBayNumber,
            requestedBayType,
            timeRange,
            licensePlate,
            touristName,
            touristEmail,
            isGuestReservation,
            totalPrice,
            nowUtc,
            nowUtc.Add(holdDuration));

        reservation.AddDomainEvent(new ReservationHeldDomainEvent(
            reservation.Id,
            reservation.ParkingLotId,
            reservation.ParkingBayId,
            reservation.AssignedBayNumber.Value,
            reservation.TimeRange.StartUtc,
            reservation.TimeRange.EndUtc,
            nowUtc));

        return reservation;
    }

    public bool Overlaps(TimeRange other) => TimeRange.Overlaps(other);

    public void ConfirmPayment(DateTimeOffset confirmedAtUtc)
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
        {
            throw new InvalidOperationException("Only active reservations can be confirmed.");
        }

        Status = ReservationStatus.Confirmed;
        ConfirmedAtUtc = confirmedAtUtc;
        ResolutionNote = null;
        AddDomainEvent(new ReservationConfirmedDomainEvent(Id, ParkingLotId, ParkingBayId, AssignedBayNumber.Value, confirmedAtUtc));
    }

    public void MarkCheckedIn(DateTimeOffset checkedInAtUtc)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed reservations can be checked in.");
        }

        Status = ReservationStatus.CheckedIn;
        CheckedInAtUtc = checkedInAtUtc;
    }

    public void ExpireHold(DateTimeOffset nowUtc)
    {
        if (Status == ReservationStatus.Held && HoldExpiresAtUtc <= nowUtc)
        {
            Status = ReservationStatus.Expired;
        }
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
    }

    public void Reassign(Guid newParkingBayId, BayNumber newBayNumber, ReassignmentReason reason, DateTimeOffset nowUtc)
    {
        if (CheckedInAtUtc is not null)
        {
            throw new InvalidOperationException("Checked in reservations cannot be reassigned.");
        }

        if (Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
        {
            throw new InvalidOperationException("Inactive reservations cannot be reassigned.");
        }

        var previousBayId = ParkingBayId;
        ParkingBayId = newParkingBayId;
        AssignedBayNumber = newBayNumber;
        LastReassignmentReason = reason;
        ResolutionNote = null;
        Status = Status == ReservationStatus.NeedsResolution ? ReservationStatus.Confirmed : Status;

        AddDomainEvent(new ReservationReassignedDomainEvent(
            Id,
            ParkingLotId,
            previousBayId,
            newParkingBayId,
            newBayNumber.Value,
            reason,
            nowUtc));
    }

    public void MarkNeedsResolution(string note)
    {
        Status = ReservationStatus.NeedsResolution;
        ResolutionNote = note;
    }
}
