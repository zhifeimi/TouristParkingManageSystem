using TPMS.Domain.Common;
using TPMS.Domain.Enums;
using TPMS.Domain.Events;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class ParkingBay : AggregateRoot<Guid>
{
    private ParkingBay()
    {
    }

    public ParkingBay(Guid id, Guid parkingLotId, BayNumber bayNumber, BayType bayType)
        : base(id)
    {
        ParkingLotId = parkingLotId;
        BayNumber = bayNumber;
        BayType = bayType;
        IsActive = true;
        OccupancyStatus = OccupancyStatus.Vacant;
        AvailabilityTouchedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid ParkingLotId { get; private set; }

    public BayNumber BayNumber { get; private set; } = new("UNASSIGNED");

    public BayType BayType { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsUnderMaintenance { get; private set; }

    public OccupancyStatus OccupancyStatus { get; private set; }

    public string? OccupiedByLicensePlate { get; private set; }

    public DateTimeOffset AvailabilityTouchedAtUtc { get; private set; }

    public void TouchAvailability(DateTimeOffset now)
    {
        AvailabilityTouchedAtUtc = now;
    }

    public void SetMaintenance(bool isUnderMaintenance, DateTimeOffset now)
    {
        IsUnderMaintenance = isUnderMaintenance;
        TouchAvailability(now);
    }

    public void MarkOccupied(LicensePlate licensePlate, DateTimeOffset now)
    {
        OccupancyStatus = OccupancyStatus.Occupied;
        OccupiedByLicensePlate = licensePlate.Value;
        TouchAvailability(now);
        AddDomainEvent(new BayOccupancyChangedDomainEvent(ParkingLotId, Id, BayNumber.Value, OccupancyStatus.Occupied, now));
    }

    public void MarkVacant(DateTimeOffset now)
    {
        OccupancyStatus = OccupancyStatus.Vacant;
        OccupiedByLicensePlate = null;
        TouchAvailability(now);
        AddDomainEvent(new BayOccupancyChangedDomainEvent(ParkingLotId, Id, BayNumber.Value, OccupancyStatus.Vacant, now));
    }
}
