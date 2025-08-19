using AdminWebTemplate.Application.DTOs.Roles;
using AdminWebTemplate.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminWebTemplate.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.AdminAccess)]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        #region Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var result = new List<RoleDto>();

            foreach (var r in roles)
            {
                var usersCount = (await _userManager.GetUsersInRoleAsync(r.Name!)).Count;
                result.Add(new RoleDto
                {
                    Id = r.Id,
                    RoleName = r.Name!,
                    UsersCount = usersCount
                });
            }

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
                return BadRequest("El nombre del rol es obligatorio.");

            if (await _roleManager.RoleExistsAsync(request.RoleName))
                return Conflict("El rol ya existe.");

            var create = await _roleManager.CreateAsync(new IdentityRole(request.RoleName));

            if (!create.Succeeded)
                return BadRequest(create.Errors);

            var role = await _roleManager.FindByNameAsync(request.RoleName);
            var dto = new RoleDto { Id = role!.Id, RoleName = role.Name!, UsersCount = 0 };

            return CreatedAtAction(nameof(GetAllRoles), new { }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> RenameRole(string id, [FromBody] RoleResponse request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
                return BadRequest("El nuevo nombre es obligatorio.");

            var role = await _roleManager.FindByIdAsync(id);
            if (role is null) return NotFound("Rol no encontrado.");

            if (!string.Equals(role.Name, request.RoleName, StringComparison.OrdinalIgnoreCase)
                && await _roleManager.RoleExistsAsync(request.RoleName))
                return Conflict("Ya existe otro rol con ese nombre.");

            role.Name = request.RoleName;
            role.NormalizedName = _roleManager.NormalizeKey(request.RoleName);

            var res = await _roleManager.UpdateAsync(role);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Rol no encontrado.");

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (users.Any())
                return Conflict($"No se puede eliminar: {users.Count} usuario(s) lo tienen asignado.");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
        #endregion

        #region Permissions

        [HttpGet("{roleName}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) return NotFound("Rol no encontrado.");

            var claims = await _roleManager.GetClaimsAsync(role);
            var perms = claims.Where(c => c.Type == "perm").Select(c => c.Value).Distinct().ToList();
            return Ok(perms);
        }

        public record PermissionsUpdateRequest(string[] Permissions);

        [HttpPut("{roleName}/permissions")]
        public async Task<IActionResult> SetRolePermissions(string roleName, [FromBody] PermissionsUpdateRequest body)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) return NotFound("Rol no encontrado.");

            var desired = new HashSet<string>(body?.Permissions ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var current = currentClaims
                .Where(c => c.Type == "perm")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var toAdd = desired.Except(current);
            var toRemove = current.Except(desired);

            foreach (var p in toAdd)
            {
                var r = await _roleManager.AddClaimAsync(role, new Claim("perm", p));
                if (!r.Succeeded) return BadRequest(r.Errors);
            }

            foreach (var p in toRemove)
            {
                var r = await _roleManager.RemoveClaimAsync(role, new Claim("perm", p));
                if (!r.Succeeded) return BadRequest(r.Errors);
            }

            return NoContent();
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var roles = _roleManager.Roles.ToList();
            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var c in claims)
                    if (c.Type == "perm" && !string.IsNullOrWhiteSpace(c.Value))
                        all.Add(c.Value);
            }

            return Ok(all.ToArray());
        }
        #endregion
    }

}
