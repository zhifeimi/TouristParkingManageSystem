using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TPMS.Api.Extensions;
using TPMS.Infrastructure.Auth;

namespace TPMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    TokenFactory tokenFactory) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenFactory.Create(user, roles.ToArray());

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                Roles = roles
            }
        });
    }

    public sealed record LoginRequest(string Email, string Password);
}
