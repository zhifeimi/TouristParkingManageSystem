namespace TPMS.Api.Extensions;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TPMS";

    public string Audience { get; set; } = "TPMS.Web";

    public string Key { get; set; } = "TPMS-Development-Key-Needs-To-Be-Changed";

    public int ExpiryMinutes { get; set; } = 480;
}
