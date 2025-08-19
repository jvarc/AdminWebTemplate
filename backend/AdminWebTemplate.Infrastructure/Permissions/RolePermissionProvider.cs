using AdminWebTemplate.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AdminWebTemplate.Infrastructure.Permissions
{
    public sealed class RolePermissionProvider(RoleManager<IdentityRole> roleManager) : IRolePermissionProvider
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<IReadOnlyCollection<string>> GetPermissionsForRoles(IEnumerable<string> roles, CancellationToken ct = default)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var roleName in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var c in claims)
                {
                    if (c.Type == "perm" && !string.IsNullOrWhiteSpace(c.Value))
                        set.Add(c.Value);
                }
            }

            return set.ToArray();
        }
    }
}
