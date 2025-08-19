using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace AdminWebTemplate.Infrastructure.Seed;

public static class IdentitySeeder
{
    private record SeedUser(
        string UserName,
        string? Email,
        string Password,
        bool EmailConfirmed,
        bool LockoutEnabled,
        string[] Roles
    );

    public static async Task SeedAsync(IServiceProvider sp)
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var seedSection = cfg.GetSection("Identity:Seed");

        var roles = seedSection.GetSection("Roles").Get<string[]>() ?? Array.Empty<string>();
        var roleClaims = seedSection.GetSection("RoleClaims")
            .Get<Dictionary<string, string[]>>() ?? new(StringComparer.OrdinalIgnoreCase);
        var users = seedSection.GetSection("Users").Get<List<SeedUser>>() ?? new();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();

        // 1) Roles
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // 2) Role Claims ("perm")
        foreach (var kv in roleClaims)
        {
            var roleName = kv.Key;
            var perms = kv.Value ?? Array.Empty<string>();

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var existing = await roleManager.GetClaimsAsync(role);
            var existingPerms = existing
                .Where(c => c.Type == "perm")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var p in perms)
            {
                if (!existingPerms.Contains(p))
                    await roleManager.AddClaimAsync(role, new Claim("perm", p));
            }
        }

        // 3) Usuarios iniciales y asignación de roles
        foreach (var su in users)
        {
            var user = await userManager.FindByNameAsync(su.UserName);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = su.UserName,
                    Email = su.Email,
                    EmailConfirmed = su.EmailConfirmed,
                    LockoutEnabled = su.LockoutEnabled
                };
                var create = await userManager.CreateAsync(user, su.Password);
                if (!create.Succeeded) continue;
            }

            // Asegurar roles del usuario
            var currentRoles = await userManager.GetRolesAsync(user);
            foreach (var r in su.Roles ?? Array.Empty<string>())
            {
                if (!currentRoles.Contains(r))
                    await userManager.AddToRoleAsync(user, r);
            }
        }
    }
}
