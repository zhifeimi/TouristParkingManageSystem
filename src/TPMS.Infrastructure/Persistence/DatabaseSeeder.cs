using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPMS.Domain.Aggregates;
using TPMS.Domain.Enums;
using TPMS.Domain.ValueObjects;
using TPMS.Infrastructure.Auth;

namespace TPMS.Infrastructure.Persistence;

public sealed class DatabaseSeeder(
    TpmsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
{
    private static readonly string[] Roles = ["Tourist", "Controller", "Operations", "Admin"];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        if (await userManager.FindByEmailAsync("admin@tpms.local") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@tpms.local",
                Email = "admin@tpms.local",
                DisplayName = "TPMS Administrator",
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(admin, "Admin123!");
            if (created.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["Admin", "Operations"]);
            }
        }

        if (await dbContext.ParkingLots.AnyAsync(cancellationToken))
        {
            return;
        }

        var lot = new ParkingLot(
            Guid.NewGuid(),
            "VALLEY-01",
            "Valley View Main Lot",
            "Australia/Sydney",
            new Money(12.50m, "AUD"));

        await dbContext.ParkingLots.AddAsync(lot, cancellationToken);

        var bayDefinitions = new[]
        {
            ("A1", BayType.Standard),
            ("A2", BayType.Standard),
            ("A3", BayType.Standard),
            ("A4", BayType.Standard),
            ("B1", BayType.Standard),
            ("B2", BayType.Standard),
            ("B3", BayType.Standard),
            ("B4", BayType.Standard),
            ("C1", BayType.Accessible),
            ("C2", BayType.EV),
            ("D1", BayType.Oversize),
            ("D2", BayType.Standard)
        };

        foreach (var (bayNumber, bayType) in bayDefinitions)
        {
            await dbContext.ParkingBays.AddAsync(
                new ParkingBay(Guid.NewGuid(), lot.Id, new BayNumber(bayNumber), bayType),
                cancellationToken);
        }

        await dbContext.EdgeNodes.AddAsync(new EdgeNode(Guid.NewGuid(), lot.Id, "EDGE-VALLEY-01"), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
