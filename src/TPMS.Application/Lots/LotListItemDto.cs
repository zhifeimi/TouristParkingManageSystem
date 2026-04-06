namespace TPMS.Application.Lots;

public sealed record LotListItemDto(
    Guid LotId,
    string Code,
    string Name,
    string TimeZoneId,
    decimal HourlyRate,
    string Currency);
