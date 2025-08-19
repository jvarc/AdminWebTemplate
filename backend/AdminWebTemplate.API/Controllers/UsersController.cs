using AdminWebTemplate.Application.DTOs.Users;
using AdminWebTemplate.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdminWebTemplate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PolicyNames.AdminAccess)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] bool includeInactive = false)
        {
            var users = _userManager.Users.ToList();

            var result = new List<UserRoleDto>();

            foreach (var user in users)
            {
                var isInactive = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

                if (!includeInactive && isInactive)
                    continue;

                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserRoleDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName!,
                    IsInactive = isInactive,
                    Roles = roles.ToList()
                });
            }

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("El nombre de usuario y la contraseña son obligatorios.");

            var existingUser = await _userManager.FindByNameAsync(request.UserName );
            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null || existingEmail != null)
                return BadRequest("El usuario ya existe.");

            var newUser = new IdentityUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser, request.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            if (request.Roles != null && request.Roles.Any())
            {
                foreach (var role in request.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                        await _userManager.AddToRoleAsync(newUser, role);
                }
            }

            return Ok(new
            {
                Message = $"Usuario '{newUser.UserName}' creado correctamente.",
                newUser.Id,
                newUser.UserName,
                RolesAsignados = request.Roles
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.UserName))
                user.UserName = request.UserName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded) return BadRequest(update.Errors);

            // Actualizar roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Except(request.Roles).ToArray();
            var toAdd = request.Roles.Except(currentRoles).ToArray();

            if (toRemove.Length > 0)
            {
                var r1 = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!r1.Succeeded) return BadRequest(r1.Errors);
            }
            if (toAdd.Length > 0)
            {
                foreach (var role in toAdd)
                    if (!await _roleManager.RoleExistsAsync(role))
                        return BadRequest($"El rol '{role}' no existe.");

                var r2 = await _userManager.AddToRolesAsync(user, toAdd);
                if (!r2.Succeeded) return BadRequest(r2.Errors);
            }

            return NoContent();
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> SetStatus(string id, [FromBody] UserStatusRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            if (request.Active)
                user.LockoutEnd = null; 
            else
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); 

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded) return BadRequest(res.Errors);

            var active = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow;
            return Ok(new { active }); 
        }

        #endregion

        #region Roles
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserAssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (!await _roleManager.RoleExistsAsync(request.RoleName))
                return BadRequest("El rol especificado no existe.");

            if (await _userManager.IsInRoleAsync(user, request.RoleName))
                return BadRequest("El usuario ya tiene este rol.");

            await _userManager.AddToRoleAsync(user, request.RoleName);
            return Ok($"Rol '{request.RoleName}' asignado a {user.UserName}.");
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] UserAssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (!await _userManager.IsInRoleAsync(user, request.RoleName))
                return BadRequest("El usuario no tiene este rol.");

            await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            return Ok($"Rol '{request.RoleName}' eliminado de {user.UserName}.");
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }
        #endregion
    }
}
