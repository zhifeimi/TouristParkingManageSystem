using TPMS.Domain.Common;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class Permit : AggregateRoot<Guid>
{
    private Permit()
    {
    }

    private Permit(
        Guid id,
        Guid reservationId,
        Guid parkingLotId,
        Guid parkingBayId,
        BayNumber bayNumber,
        LicensePlate licensePlate,
        DateTimeOffset validFromUtc,
        DateTimeOffset validToUtc,
        string permitCode)
        : base(id)
    {
        ReservationId = reservationId;
        ParkingLotId = parkingLotId;
        ParkingBayId = parkingBayId;
        BayNumber = bayNumber;
        LicensePlate = licensePlate;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        PermitCode = permitCode;
        Status = PermitStatus.Active;
    }

    public Guid ReservationId { get; private set; }

    public Guid ParkingLotId { get; private set; }

    public Guid ParkingBayId { get; private set; }

    public BayNumber BayNumber { get; private set; } = new("UNASSIGNED");

    public LicensePlate LicensePlate { get; private set; } = new("UNKNOWN");

    public DateTimeOffset ValidFromUtc { get; private set; }

    public DateTimeOffset ValidToUtc { get; private set; }

    public string PermitCode { get; private set; } = string.Empty;

    public PermitStatus Status { get; private set; }

    public static Permit IssueFromReservation(Reservation reservation)
    {
        return new Permit(
            Guid.NewGuid(),
            reservation.Id,
            reservation.ParkingLotId,
            reservation.ParkingBayId,
            reservation.AssignedBayNumber,
            reservation.LicensePlate,
            reservation.TimeRange.StartUtc,
            reservation.TimeRange.EndUtc,
            $"PERMIT-{reservation.Id.ToString("N")[..8].ToUpperInvariant()}");
    }

    public bool IsValidFor(LicensePlate licensePlate, DateTimeOffset nowUtc)
    {
        return Status == PermitStatus.Active &&
               LicensePlate == licensePlate &&
               ValidFromUtc <= nowUtc &&
               nowUtc <= ValidToUtc;
    }

    public void Suspend()
    {
        Status = PermitStatus.Suspended;
    }

    public void Expire()
    {
        Status = PermitStatus.Expired;
    }
}
