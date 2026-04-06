using System.Diagnostics;

namespace TPMS.Application.Common;

public static class Telemetry
{
    public const string ActivitySourceName = "TPMS.Application";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
