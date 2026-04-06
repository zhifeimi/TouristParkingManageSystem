using Microsoft.AspNetCore.Identity;

namespace TPMS.Infrastructure.Auth;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
