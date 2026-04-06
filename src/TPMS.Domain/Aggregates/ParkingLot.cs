using TPMS.Domain.Common;
using TPMS.Domain.ValueObjects;

namespace TPMS.Domain.Aggregates;

public sealed class ParkingLot : AggregateRoot<Guid>
{
    private ParkingLot()
    {
    }

    public ParkingLot(Guid id, string code, string name, string timeZoneId, Money defaultHourlyRate)
        : base(id)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        TimeZoneId = timeZoneId.Trim();
        DefaultHourlyRate = defaultHourlyRate;
        IsActive = true;
        AvailabilityTouchedAtUtc = DateTimeOffset.UtcNow;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string TimeZoneId { get; private set; } = "Australia/Sydney";

    public Money DefaultHourlyRate { get; private set; } = Money.Zero();

    public bool IsActive { get; private set; }

    public DateTimeOffset AvailabilityTouchedAtUtc { get; private set; }

    public void TouchAvailability(DateTimeOffset now)
    {
        AvailabilityTouchedAtUtc = now;
    }

    public Money CalculateReservationPrice(TimeRange timeRange)
    {
        var billableHours = Math.Max(1m, decimal.Ceiling((decimal)timeRange.Duration.TotalMinutes / 60m));
        return DefaultHourlyRate.Multiply(billableHours);
    }
}
