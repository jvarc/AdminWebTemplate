

namespace AdminWebTemplate.Application.Interfaces
{
    public sealed record TokenRequest(
      string UserId,
      string UserName,
      IEnumerable<string> Roles,
      IEnumerable<System.Security.Claims.Claim>? ExtraClaims = null);

    public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        TokenResult Generate(TokenRequest request);
    }
}
