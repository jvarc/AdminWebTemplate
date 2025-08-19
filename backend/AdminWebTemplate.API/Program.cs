using AdminWebTemplate.Infrastructure;
using AdminWebTemplate.Infrastructure.Seed;
using AdminWebTemplate.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- JWT config & validaciones ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var keyRaw = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(keyRaw))
    throw new InvalidOperationException("Missing JWT settings. Set Jwt:Issuer, Jwt:Audience and Jwt:Key (via user-secrets in Development).");

if (keyRaw.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters (256-bit recommended).");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));

// --- CORS ---
var cors = builder.Configuration.GetSection("Cors");
var corsPolicyName = cors["PolicyName"] ?? "DefaultCors";
var allowedOrigins = cors.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, p =>
    {
        if (allowedOrigins.Length > 0)
        {
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        }
        else
        {
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
    });
});

// --- Infraestructura (DbContext/Identity configurable: SqlServer/Sqlite/SqliteInMemory) ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- MVC + Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- AuthN/AuthZ ---
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminAccess", p => p.RequireClaim("perm", "admin:access"))
    .AddPolicy("Users_Read", p => p.RequireClaim("perm", "users:read"))
    .AddPolicy("Users_Write", p => p.RequireClaim("perm", "users:write"));

var app = builder.Build();

// --- Migraciones + Seeding + Demo (antes del pipeline) ---
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";

    var db = sp.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        if (string.Equals(provider, "SqliteInMemory", StringComparison.OrdinalIgnoreCase))
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();
    }

    await IdentitySeeder.SeedAsync(sp);

    // 3) Modo demo (opcional): crea admin efímero si no hay usuarios
    var cfg = sp.GetRequiredService<IConfiguration>();
    if (cfg.GetValue<bool>("Demo:Enabled"))
    {
        var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();

        var email = cfg["Demo:AdminEmail"] ?? "admin@demo.local";
        var pass = cfg["Demo:AdminPassword"] ?? "Demo123$";

        var user = await userMgr.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true, LockoutEnabled = false };
            var created = await userMgr.CreateAsync(user, pass); // crea con hash correcto
            if (!created.Succeeded)
                throw new InvalidOperationException("Demo admin creation failed: " + string.Join(", ", created.Errors.Select(e => e.Description)));
        }
        else
        {
            // Asegura la contraseña demo sin tokens
            if (await userMgr.HasPasswordAsync(user))
            {
                var removed = await userMgr.RemovePasswordAsync(user);
                if (!removed.Succeeded)
                    throw new InvalidOperationException("RemovePassword failed: " + string.Join(", ", removed.Errors.Select(e => e.Description)));
            }

            var added = await userMgr.AddPasswordAsync(user, pass);
            if (!added.Succeeded)
                throw new InvalidOperationException("AddPassword failed: " + string.Join(", ", added.Errors.Select(e => e.Description)));
        }

    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
