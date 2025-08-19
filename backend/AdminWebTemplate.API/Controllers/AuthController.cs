using AdminWebTemplate.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace AdminWebTemplate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRolePermissionProvider _permProvider; 

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenService tokenService,
            IRolePermissionProvider permProvider) 
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _permProvider = permProvider;
        }


        /// <summary>
        /// Inicia sesión y devuelve un token JWT con claims de permisos (perm)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized("Usuario o contraseña incorrectos");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Cuenta inactiva" });

            var roles = await _userManager.GetRolesAsync(user);

            var perms = await _permProvider.GetPermissionsForRoles(roles);

            var extraClaims = perms.Select(p => new Claim("perm", p));

            var token = _tokenService.Generate(new TokenRequest(
                user.Id,
                user.UserName!,
                roles,
                extraClaims
            ));

            return Ok(new
            {
                access_token = token.AccessToken,
                token_type = "Bearer",
                expires_at = token.ExpiresAtUtc.ToUniversalTime().ToString("o")
            });
        }
    }



    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
