namespace TPMS.Application.Common;

public sealed class AvailabilityConcurrencyException(
    string message,
    Guid? lotId,
    DateTimeOffset? startUtc,
    DateTimeOffset? endUtc,
    Exception? innerException = null) : Exception(message, innerException)
{
    public Guid? LotId { get; } = lotId;

    public DateTimeOffset? StartUtc { get; } = startUtc;

    public DateTimeOffset? EndUtc { get; } = endUtc;
}
