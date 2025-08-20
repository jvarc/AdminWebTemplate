### 📄 `backend/README.md`

# AdminWebTemplate – Backend (.NET 8 Web API)

## Tech

- ASP.NET Core Web API (.NET 8)
- Identity + JWT (claims `perm` y `role`)
- EF Core (SQLite en memoria / archivo, o SQL Server)
- Seeding: roles/claims desde `appsettings.json` y admin demo

## Arranque rápido (demo)

> Sin instalar nada adicional.

1. Abre `AdminWebTemplate.API` en Visual Studio y ejecuta **Run**.
2. Por defecto usa **SQLite en memoria** y crea el usuario demo:
   - Email: `admin@demo.local`
   - Password: `Demo123$`
3. Swagger está disponible en **Development**.

## Configuración (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "__READ_FROM_USER_SECRETS__",
    "SqliteFile": "Data Source=admin.db"
  },
  "Database": { "Provider": "SqliteInMemory" }, // SqliteInMemory | Sqlite | SqlServer
  "Jwt": {
    "Issuer": "AdminWebTemplate",
    "Audience": "AdminWebTemplateUsers",
    "Key": "__READ_FROM_USER_SECRETS__",
    "ExpireMinutes": 60
  },
  "Cors": {
    "PolicyName": "AllowAdminWebTemplateFrontend",
    "AllowedOrigins": ["http://localhost:4200"]
  },
  "Identity": {
    "Seed": {
      "Roles": ["Admin", "Manager", "User"],
      "RoleClaims": {
        "Admin": ["admin:access", "users:read", "users:write"],
        "Manager": ["users:read"],
        "User": []
      }
    }
  },
  "Demo": {
    "Enabled": true,
    "AdminEmail": "admin@demo.local",
    "AdminPassword": "Demo123$"
  }
}
```

## User Secrets (desarrollo)

Visual Studio → Manage User Secrets:

json
Copiar
Editar

```json
{
  "Jwt": { "Key": "AL_MENOS_32_CARACTERES" },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
  }
}
```

## Proveedores de base de datos

#### SqliteInMemory (demo): EnsureCreated() en startup, sin migraciones, datos efímeros.

#### Sqlite (archivo): persistencia local (admin.db), WAL habilitado para concurrencia.

#### SqlServer: producción; usa DefaultConnection en secrets.

#### Si usas persistencia, añade/usa migraciones (Infrastructure/Migrations).

## Endpoints principales

#### POST /api/auth/login → { email, password } ⇒ { token, claims }

#### GET /api/users (perm users:read)

#### POST /api/users (perm users:write)

#### PUT /api/users/{id}/activate|deactivate (perm users:write)

#### GET /api/roles (perm users:read)

#### GET /api/roles/permissions (perm users:read) → lista de perm

## Seguridad

Nunca publiques Jwt:Key ni cadenas reales.

#### .gitignore ignora SQLite (_.db, _-wal, _-shm), appsettings._.json de entornos, certs, etc.

Para dominios reales en CORS, configúralos por entorno (no en el repo).

## EF Core (si eliges persistencia)

#### (opcional) crear migración inicial

Copiar
Editar

```bash
pgsql
dotnet ef migrations add InitialCreate -p AdminWebTemplate.Infrastructure -s AdminWebTemplate.API -o Migrations
dotnet ef database update -p AdminWebTemplate.Infrastructure -s AdminWebTemplate.API
```
