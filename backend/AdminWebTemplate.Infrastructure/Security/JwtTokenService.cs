using AdminWebTemplate.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AdminWebTemplate.Infrastructure.Security
{
    public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
    {
        private readonly JwtOptions _opt = options.Value;

        public TokenResult Generate(TokenRequest req)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, req.UserId),
                new(ClaimTypes.Name, req.UserName),
                new(JwtRegisteredClaimNames.Name, req.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(req.Roles.Select(r => new Claim(ClaimTypes.Role, r)));


            if (req.ExtraClaims is not null)
                claims.AddRange(req.ExtraClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_opt.ExpireMinutes),
                signingCredentials: creds
            );

            var handler = new JwtSecurityTokenHandler();
            return new(handler.WriteToken(token), token.ValidTo);
        }

    }
}
