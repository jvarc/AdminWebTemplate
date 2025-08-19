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

    var db = sp.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(sp);

    // 3) Modo demo (opcional): crea admin ef�mero si no hay usuarios
    var cfg = sp.GetRequiredService<IConfiguration>();
    if (cfg.GetValue<bool>("Demo:Enabled"))
    {
        var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();
        if (!userMgr.Users.Any())
        {
            var email = cfg["Demo:AdminEmail"] ?? "admin@demo.local";
            var pass = cfg["Demo:AdminPassword"] ?? "Demo123$";

            var u = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true, LockoutEnabled = false };
            await userMgr.CreateAsync(u, pass);

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleMgr.RoleExistsAsync("Admin"))
                await roleMgr.CreateAsync(new IdentityRole("Admin"));

            await userMgr.AddToRoleAsync(u, "Admin");
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
