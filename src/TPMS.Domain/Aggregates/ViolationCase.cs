using TPMS.Domain.Common;
using TPMS.Domain.Enums;
using TPMS.Domain.Events;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class ViolationCase : AggregateRoot<Guid>
{
    private ViolationCase()
    {
    }

    public ViolationCase(
        Guid id,
        Guid parkingLotId,
        Guid? parkingBayId,
        BayNumber? bayNumber,
        LicensePlate licensePlate,
        string reason,
        string details,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        ParkingLotId = parkingLotId;
        ParkingBayId = parkingBayId;
        BayNumber = bayNumber;
        LicensePlate = licensePlate;
        Reason = reason.Trim();
        Details = details.Trim();
        Status = ViolationStatus.Open;
        CreatedAtUtc = createdAtUtc;
        AddDomainEvent(new ViolationRaisedDomainEvent(Id, parkingLotId, parkingBayId, licensePlate.Value, Reason, createdAtUtc));
    }

    public Guid ParkingLotId { get; private set; }

    public Guid? ParkingBayId { get; private set; }

    public BayNumber? BayNumber { get; private set; }

    public LicensePlate LicensePlate { get; private set; } = new("UNKNOWN");

    public string Reason { get; private set; } = string.Empty;

    public string Details { get; private set; } = string.Empty;

    public ViolationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public void Close(DateTimeOffset closedAtUtc)
    {
        Status = ViolationStatus.Closed;
        ClosedAtUtc = closedAtUtc;
    }
}
