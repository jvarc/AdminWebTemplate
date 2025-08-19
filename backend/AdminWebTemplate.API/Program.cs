using AdminWebTemplate.Infrastructure;
using AdminWebTemplate.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var keyRaw = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(keyRaw))
    throw new InvalidOperationException("Missing JWT settings. Set Jwt:Issuer, Jwt:Audience and Jwt:Key (via user-secrets in Development).");

if (keyRaw.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters (256-bit recommended).");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));

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
            // Desarrollo o ejemplo sin orígenes definidos
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
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
