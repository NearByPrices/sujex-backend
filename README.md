# ErpCrm.API - Backend (ASP.NET Core 8)

Multi-tenant ERP + CRM platform backend built with ASP.NET Core 8, PostgreSQL (via Supabase), and Entity Framework Core.

## Project Structure

```
src/
├── ErpCrm.API/              # REST API, controllers, middleware, Program.cs
├── ErpCrm.Application/      # DTOs, validators, use cases, interfaces
├── ErpCrm.Domain/           # Entities, enums, domain rules (no dependencies)
└── ErpCrm.Infrastructure/   # EF Core, Dapper, JWT, email, audit implementations

tests/
├── ErpCrm.UnitTests/        # Unit tests
└── ErpCrm.IntegrationTests/ # Integration tests with real DB
```

## Architecture

- **Clean Layered Architecture** — Domain → Application → Infrastructure → API
- **Multi-tenancy** — Single database, row-level isolation via `TenantId`
- **Entity Framework Core** with Npgsql for PostgreSQL via Supabase
- **Dapper** for complex read queries
- **JWT + Refresh Token Rotation** for authentication
- **ASP.NET Core Identity** for user/role management
- **Serilog** for structured logging

## Prerequisites

- **.NET 8 SDK** installed
- **PostgreSQL client** (psql or TablePlus) for migrations
- **Supabase project** created with database URL and credentials

## Setup

### 1. Clone & Install

```bash
cd backend
dotnet restore
```

### 2. Configure Connection Strings

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.YOUR_REF.supabase.co;Database=postgres;Username=postgres.YOUR_REF;Password=YOUR_PASSWORD;Port=6543;SSL Mode=Require;Trust Server Certificate=true",
    "MigrationConnection": "Host=db.YOUR_REF.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters-long",
    "Issuer": "erp-crm-api",
    "Audience": "erp-crm-clients"
  }
}
```

**Important:** Add `appsettings.*.json` to `.gitignore` — never commit secrets.

### 3. Run Initial Migration

```bash
# From backend directory
dotnet ef database update \
  --project src/ErpCrm.Infrastructure \
  --startup-project src/ErpCrm.API \
  --configuration MigrationConnection
```

### 4. Run Development Server

```bash
dotnet run --project src/ErpCrm.API
```

API will start at `https://localhost:5001` and `http://localhost:5000`

Swagger/OpenAPI docs: `http://localhost:5000/swagger`

## Database

### Supabase Connection

Use the **pooled connection string** (port 6543) for runtime:
```
Server=db.YOUR_REF.supabase.co;Database=postgres;User ID=postgres.YOUR_REF;Password=YOUR_PASSWORD;Port=6543;SSL Mode=Require;
```

Use the **direct connection string** (port 5432) for EF Core migrations only:
```
Server=db.YOUR_REF.supabase.co;Database=postgres;User ID=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;
```

### Key Conventions

| Field | Type | Notes |
|-------|------|-------|
| Primary Keys | `uuid` | `Guid` in C# |
| Money/Quantities | `numeric(18, 4)` | `decimal` in C# |
| Timestamps | `timestamptz` | Always UTC |
| JSON | `jsonb` | Indexed, queryable |
| Table Names | `snake_case` | EF Core auto-maps via `UseSnakeCaseNamingConvention()` |
| Soft Deletes | `is_deleted boolean` | Only where business requires |

### Multi-Tenancy

Every business entity has a `TenantId` column. EF Core global query filters enforce tenant isolation automatically.

```csharp
// Example: All Company queries automatically filtered by TenantId
var companies = await dbContext.Companies.ToListAsync();
// Generated SQL: WHERE tenant_id = @currentTenantId
```

## API Conventions

### Request/Response Format

All responses use a consistent wrapper:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": null,
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100
  }
}
```

### Versioning

- Platform Admin: `/api/platform/auth/login`, `/api/platform/tenants`
- Tenant User: `/api/v1/auth/login`, `/api/v1/companies`, `/api/v1/crm/contacts`

## Auth

### Platform Admin Login
```http
POST /api/platform/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "password"
}
```

Response: `{ "token": "jwt...", "refreshToken": "..." }`

JWT claims:
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim("email", user.Email),
new Claim("role", role.Name),
new Claim("tenant_id", "null")  // Platform admin has no tenant
```

### Tenant User Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@company.com",
  "password": "password",
  "tenantSlug": "company-slug"
}
```

JWT claims:
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim("email", user.Email),
new Claim("tenant_id", user.TenantId.ToString()),
new Claim("company_id", userCompanyRole.CompanyId.ToString()),
new Claim("role", role.Name)
```

## Phase 1 Deliverables

- [x] Domain entities (BaseEntity, TenantEntity, Platform, Identity, Foundation)
- [ ] AppDbContext with EF Core config
- [ ] Initial migration
- [ ] TenantContext middleware
- [ ] ExceptionHandlingMiddleware
- [ ] Platform auth endpoints
- [ ] Tenant user auth endpoints
- [ ] Company CRUD
- [ ] User CRUD
- [ ] Swagger configured

## NuGet Packages

```
Npgsql.EntityFrameworkCore.PostgreSQL
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Dapper
Npgsql
Serilog.AspNetCore
Serilog.Sinks.Console
Serilog.Sinks.File
FluentValidation.AspNetCore
Swashbuckle.AspNetCore
AutoMapper.Extensions.Microsoft.DependencyInjection
```

## Coding Rules

1. **Never expose EF entities** — always use DTOs
2. **Business logic in services**, not controllers
3. **All DB operations async** (`async/await`)
4. **Decimal for money** — `numeric(18, 4)` in DB
5. **UTC for timestamps** — `DateTime.UtcNow`
6. **Transactions** for multi-table operations
7. **No manual tenant filtering** — EF global filters handle it
8. **Dapper only for complex reports**
9. **One migration per change**
10. **Keep controllers thin** — one method = one service call

## Debugging

### Enable SQL Logging

In `Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(...)
        .LogTo(Console.WriteLine, LogLevel.Information)
);
```

### Test Database Connection

```csharp
var connection = new NpgsqlConnection(connectionString);
connection.Open();
Console.WriteLine("Connected!");
```

## Resources

- [ASP.NET Core docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [EF Core Npgsql](https://www.npgsql.org/efcore/)
- [Supabase PostgreSQL](https://supabase.com/docs/guides/database)
- [JWT Bearer](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.securitytokenhandler)

---

**Status:** Phase 1 setup in progress
