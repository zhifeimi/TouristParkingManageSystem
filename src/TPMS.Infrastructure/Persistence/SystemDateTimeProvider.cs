using TPMS.Application.Abstractions;

namespace TPMS.Infrastructure.Persistence;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
