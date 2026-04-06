namespace TPMS.Application.Common;

public interface IAvailabilityWindowRequest
{
    Guid LotId { get; }

    DateTimeOffset StartUtc { get; }

    DateTimeOffset EndUtc { get; }
}
