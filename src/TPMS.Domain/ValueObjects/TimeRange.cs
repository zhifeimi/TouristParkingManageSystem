namespace TPMS.Domain.ValueObjects;

public sealed record TimeRange
{
    public TimeRange(DateTimeOffset startUtc, DateTimeOffset endUtc)
    {
        if (endUtc <= startUtc)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        StartUtc = startUtc.ToUniversalTime();
        EndUtc = endUtc.ToUniversalTime();
    }

    public DateTimeOffset StartUtc { get; }

    public DateTimeOffset EndUtc { get; }

    public TimeSpan Duration => EndUtc - StartUtc;

    public bool Overlaps(TimeRange other) => StartUtc < other.EndUtc && other.StartUtc < EndUtc;
}
