using AdminWebTemplate.Application.Interfaces;
using AdminWebTemplate.Infrastructure.Permissions;
using AdminWebTemplate.Infrastructure.Persistence;
using AdminWebTemplate.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace AdminWebTemplate.Infrastructure
{
    public static class DependencyInjection
    {
        private static DbConnection? _sqliteInMemoryConnection;

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtOptions>(config.GetSection("Jwt"));

            // --- selección de proveedor ---
            var provider = config["Database:Provider"] ?? "SqlServer";   // SqlServer | Sqlite | SqliteInMemory
            var sqlServerCs = config.GetConnectionString("DefaultConnection");
            var sqliteFileCs = config.GetConnectionString("SqliteFile") ?? "Data Source=admin.db";

            services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            {
                switch (provider)
                {
                    case "SqliteInMemory":
                        _sqliteInMemoryConnection ??= new SqliteConnection("Data Source=:memory:");
                        if (_sqliteInMemoryConnection.State != System.Data.ConnectionState.Open)
                            _sqliteInMemoryConnection.Open();
                        opt.UseSqlite(_sqliteInMemoryConnection, b =>
                            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                        break;

                    case "Sqlite":
                        var builder = new SqliteConnectionStringBuilder(sqliteFileCs)
                        {
                            Cache = SqliteCacheMode.Shared,
                            DefaultTimeout = 30
                        };
                        var sqliteConn = new SqliteConnection(builder.ToString());
                        sqliteConn.Open();

                        // Activar WAL (queda persistido en el archivo)
                        using (var cmd = sqliteConn.CreateCommand())
                        {
                            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
                            cmd.ExecuteNonQuery();
                        }

                        opt.UseSqlite(sqliteConn, b =>
                            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                        break;

                    default: // SqlServer
                        opt.UseSqlServer(sqlServerCs, b =>
                            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                        break;
                }
            });

            services.AddIdentityCore<IdentityUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<IRolePermissionProvider, RolePermissionProvider>();

            return services;
        }
    }
}
