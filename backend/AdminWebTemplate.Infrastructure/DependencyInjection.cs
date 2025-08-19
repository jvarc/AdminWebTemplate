using AdminWebTemplate.Application.Interfaces;
using AdminWebTemplate.Infrastructure.Permissions;
using AdminWebTemplate.Infrastructure.Persistence;
using AdminWebTemplate.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdminWebTemplate.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtOptions>(config.GetSection("Jwt"));

            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<IdentityUser>()
                  .AddRoles<IdentityRole>()
                  .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<IRolePermissionProvider, RolePermissionProvider>();

            return services;
        }
    }
}
