
namespace AdminWebTemplate.Infrastructure.Security
{
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public double ExpireMinutes { get; init; } = 60;
    }
}
