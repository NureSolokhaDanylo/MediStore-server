# MediStore Server - Copilot Instructions

This is a healthcare inventory management system with IoT sensor integration for monitoring medicine storage conditions.

## Architecture

**Clean Architecture with 4 layers:**

```
WebApi (Presentation)
  ↓
Application (Business Logic)
  ↓
Infrastructure (Data Access)
  ↓
Domain/Entities (Core Models)
```

**Dependency flow:** All dependencies point inward. Infrastructure and Application both reference Domain, but Domain has no dependencies on other layers.

### Project Responsibilities

- **Domain (Entities/):** Pure domain models (`EntityBase`, `Medicine`, `Batch`, `Sensor`, `Zone`, `Reading`, `Alert`, etc.). No framework dependencies.
- **Infrastructure:** EF Core `AppDbContext`, Repository implementations, migrations, `UnitOfWork`, retry policies
- **Application:** Service layer (`ICrudService<T>`, `IReadOnlyService<T>`), DTOs, Result pattern, background hosted services, seeders
- **WebApi:** Controllers, API DTOs, JWT authentication setup, Swagger configuration, startup composition
- **SharedConfiguration:** Centralized configuration options (`JwtOptions`, `InfrastructureOptions`, `SeedOptions`)

## Build & Run

### Using Docker (Recommended)

**Linux:**
```bash
# First time: Create Scripts/path_to_env file with absolute path to your .env file
echo "/path/to/your/dev.env" > Scripts/path_to_env

# Start services (builds automatically)
./Scripts/lin/up.sh

# Run in background
./Scripts/lin/up_detached.sh

# Recreate database from scratch
./Scripts/lin/recreate_db.sh

# Stop services
./Scripts/lin/down.sh
```

**Windows:**
```cmd
Scripts\win\up.cmd
Scripts\win\up_detached.cmd
Scripts\win\recreate_db.cmd
Scripts\win\down.cmd
```

**Required environment variables** (see `compose.template.env`):
- `SERVER_PORT` - API port
- `DB_PORT` - SQL Server port
- `DB_NAME`, `DB_USER`, `DB_PASSWORD` - Database credentials
- `JwtOptions__Key` - JWT signing key
- `SeedOptions__AdminLogin`, `SeedOptions__AdminPassword` - Initial admin account

### Using .NET CLI

```bash
# Build solution
dotnet build MediStore-server.sln

# Run WebApi project
dotnet run --project WebApi

# Run specific project
dotnet run --project SensorsEmulator
```

### Database Migrations

```bash
# Create migration (from repo root)
dotnet ef migrations add MigrationName --project Infrastructure --startup-project WebApi

# Apply migrations (happens automatically on startup, or manually:)
dotnet ef database update --project Infrastructure --startup-project WebApi

# Remove last migration
dotnet ef migrations remove --project Infrastructure --startup-project WebApi
```

**Note:** Migrations are in `Infrastructure/Migrations/`. The application automatically applies pending migrations on startup.

## Key Patterns & Conventions

### Result Pattern (No Exceptions for Business Logic)

All Application services return `Result<T>` or `Result` instead of throwing exceptions:

```csharp
// Application/Results/Base/Result.cs
public class Result 
{
    public bool IsSucceed { get; }
    public string? ErrorMessage { get; }
}

public class Result<T> : Result 
{
    public T? Value { get; }
}
```

**Usage:**
```csharp
var result = await _service.Add(userId, entity);
if (!result.IsSucceed) return BadRequest(result.ErrorMessage);
var created = result.Value!;
```

### Repository + Unit of Work

**All data access goes through `IUnitOfWork`:**

```csharp
public interface IUnitOfWork
{
    IMedicineRepository Medicines { get; }
    ISensorRepository Sensors { get; }
    IBatchRepository Batches { get; }
    IReadingRepository Readings { get; }
    // ... other repositories
    
    Task<int> SaveChangesAsync();
}
```

**Never use `AppDbContext` directly in services.** Always inject `IUnitOfWork`.

**Repository base class:** `Repository<T> where T : EntityBase` provides CRUD operations. Specific repositories extend this for custom queries.

### Controller Hierarchy (Generic CRUD)

**Template pattern for controllers:**

```
MyController (base class - extracts userId from JWT claims)
    ↓
ReadController<TEntity, TDto, TService> 
    - GET /api/v1/resource
    - GET /api/v1/resource/{id}
    ↓
CrudController<TEntity, TDto, TCreateDto, TService>
    - POST /api/v1/resource
    - PUT /api/v1/resource
    - DELETE /api/v1/resource/{id}
```

**To add a new CRUD endpoint:**
1. Create `EntityDto` and `CreateEntityDto` in `WebApi/DTOs/`
2. Create controller inheriting from `CrudController<Entity, EntityDto, CreateEntityDto, IEntityService>`
3. Implement `ToEntity()` and `ToDto()` mapping methods
4. Add `[Route("api/v1/resource")]` and role-based `[Authorize]` attributes

**Example:**
```csharp
[Route("api/v1/batches")]
[Authorize(Roles = "Admin,Operator,Observer")]
public class BatchesController : CrudController<Batch, BatchDto, CreateBatchDto, IBatchService>
{
    public BatchesController(IBatchService service) : base(service) { }
    
    protected override Batch ToEntity(BatchDto dto) => // map DTO to entity
    protected override Batch ToEntity(CreateBatchDto dto) => // map create DTO to entity
    protected override BatchDto ToDto(Batch entity) => // map entity to DTO
    protected override int GetId(BatchDto dto) => dto.Id;
}
```

### Service Layer Convention

**Service naming:** `<Entity>Service` implements `I<Entity>Service`

**Base service interfaces:**
- `IReadOnlyService<T>` - GET operations only (`GetAll`, `Get`)
- `ICrudService<T> : IReadOnlyService<T>` - Full CRUD (`Add`, `Update`, `Delete`)

**All services:**
- Accept `userId` as first parameter (for audit logging)
- Return `Result<T>` or `Result`
- Use `IUnitOfWork` for data access
- Create audit log entries for write operations via `_auditLogger.LogAsync()`

### Authentication & Authorization

**JWT-based authentication** with ASP.NET Core Identity.

**Roles:**
- `Admin` - Full access, can manage users and app settings
- `Operator` - Can create/update/delete entities (except users)
- `Observer` - Read-only access

**Special authentication:**
- **Sensors** use API key in `X-Sensor-Api-Key` header (see `SensorApiKeyService`)
- Check `ApiKeyAuthMiddleware` for sensor authentication flow

**Extract user ID in controllers:**
```csharp
protected string? userId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

### Background Services

**Location:** `Application/Hosted/`

All background services inherit from `BackgroundService` and run continuously:

- `ExpiredChecker` - Creates alerts for expired batches
- `ExpirationSoonChecker` - Warns about soon-expiring batches (configurable threshold in `Medicine.WarningThresholdDays`)
- `BatchConditionChecker` - Monitors temperature/humidity for batches
- `ZoneConditionChecker` - Monitors storage zone conditions
- `ReadingsRetentionCleaner` - Deletes old sensor readings (data retention)

**All checkers:**
- Use `AppSettings.AlertEnabled` to enable/disable alerts
- Access intervals from `AppSettings` (e.g., `AppSettings.ExpiredCheckIntervalMs`)
- Use scoped `IUnitOfWork` within `ExecuteAsync` loop
- Handle exceptions without crashing the service

### Database Conventions

**Entity base class:**
```csharp
public abstract class EntityBase
{
    public int Id { get; set; }
}
```

All domain entities inherit from `EntityBase`.

**EF Core configuration:**
- **Lazy loading enabled** via proxies (navigation properties must be `virtual`)
- **Connection resiliency:** 10 retries, 30-second max delay
- **Command timeout:** 120 seconds
- **Schema organization:**
  - `dbo` - Main entities + Identity tables
  - `config` - AppSettings
  - `log` - AuditLog

**Relationships:**
- Most foreign keys use `OnDelete: Restrict` or `SetNull` (no cascades)
- Important index: `Reading.SensorId` (for time-series queries)

**Migrations are applied automatically** on application startup via `InitializeDatabaseAsync()` extension method.

### Dependency Injection Registration

**Extension method pattern** for clean DI registration in `Program.cs`:

```csharp
services
    .AddAppConfiguration(config)      // SharedConfiguration
    .AddInfrastructure()              // Infrastructure (DbContext, Repositories, UOW)
    .AddAppIdentity()                 // Infrastructure (ASP.NET Identity)
    .AddApplication()                 // Application (Services)
    .AddAppSeeders()                  // Application (Database seeders)
    .AddAuth()                        // WebApi (JWT authentication)
    .AddAppControllersAndSwagger()    // WebApi (Controllers, Swagger)
    .AddAppHostedServices();          // Application (Background services)
```

Each layer provides its own extension methods (e.g., `AddInfrastructure()` in `Infrastructure/AddInfrastructure.cs`).

### Seeding

**Seeders** implement `ISeeder` and are executed on startup in `Program.cs`:

```csharp
foreach (var seeder in scope.ServiceProvider.GetServices<ISeeder>())
{
    await seeder.SeedAsync();
}
```

Existing seeders:
- `IdentitySeeder` - Creates roles (Admin, Operator, Observer) and admin user
- `AppSettingsSeeder` - Initializes system configuration

**Seeders are idempotent** - safe to run multiple times.

### API Response Conventions

**Standard HTTP status codes:**
- `200 OK` - Successful GET/PUT
- `201 Created` - Successful POST (with `Location` header pointing to new resource)
- `400 Bad Request` - Validation failure, business rule violation
- `401 Unauthorized` - Missing or invalid JWT token
- `403 Forbidden` - User lacks required role
- `404 Not Found` - Resource doesn't exist

**Error responses** return `Result.ErrorMessage` in response body.

### Sensor Data Flow

1. **IoT sensors** POST readings to `/api/v1/readings` with `X-Sensor-Api-Key` header
2. **ReadingService** validates sensor, stores reading in database
3. **Background checkers** periodically analyze readings:
   - Compare against `Medicine.MinTemperature`, `MaxTemperature`, `MinHumidity`, `MaxHumidity`
   - Create `Alert` entries when conditions violated
4. **Web clients** query readings via `/api/v1/readings/sensor/{id}` or `/api/v1/readings/zone/{id}` with time range filters

### Testing

**No test projects currently exist.** When adding tests:
- Create separate test projects (e.g., `Application.Tests`, `WebApi.Tests`)
- Use xUnit as testing framework (standard for .NET)
- Mock `IUnitOfWork` and repositories for service tests
- Use `WebApplicationFactory<Program>` for integration tests

## Common Tasks

### Adding a New Entity

1. **Domain layer** (`Entities/Models/`):
   - Create entity class inheriting from `EntityBase`
   - Add navigation properties (mark as `virtual` for lazy loading)

2. **Infrastructure layer:**
   - Add `DbSet<NewEntity>` to `AppDbContext`
   - Configure relationships in `OnModelCreating()`
   - Create repository interface `INewEntityRepository : IRepository<NewEntity>`
   - Create repository implementation `NewEntityRepository : Repository<NewEntity>, INewEntityRepository`
   - Add repository to `IUnitOfWork` and `UnitOfWork` class
   - Register in `AddInfrastructure()` DI extension
   - Create migration: `dotnet ef migrations add AddNewEntity --project Infrastructure --startup-project WebApi`

3. **Application layer:**
   - Create service interface `INewEntityService : ICrudService<NewEntity>`
   - Create service implementation `NewEntityService`
   - Register in `AddApplication()` DI extension
   - Create DTOs in `Application/DTOs/`

4. **WebApi layer:**
   - Create DTOs in `WebApi/DTOs/` (API-specific if different from Application DTOs)
   - Create controller inheriting from `CrudController<NewEntity, NewEntityDto, CreateNewEntityDto, INewEntityService>`
   - Add `[Route("api/v1/newentities")]` and `[Authorize]` attributes
   - Implement `ToEntity()` and `ToDto()` mapping methods

### Adding a Custom Endpoint

For endpoints that don't fit CRUD pattern:

```csharp
[HttpGet("custom-action")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CustomAction([FromQuery] string param)
{
    var uid = userId;
    if (string.IsNullOrEmpty(uid)) return Unauthorized();
    
    var result = await _service.CustomMethod(uid, param);
    if (!result.IsSucceed) return BadRequest(result.ErrorMessage);
    
    return Ok(result.Value);
}
```

### Debugging Connection Issues

**If database initialization fails:**
1. Check `Scripts/path_to_env` file points to valid `.env` file
2. Verify all required environment variables in `.env`
3. Ensure SQL Server container is running: `docker ps`
4. Check connection string in compose.yaml matches SQL Server settings
5. Application retries 12 times with 5-second delays - check logs for specific error

**Database initialization code:** `WebApi/Extensions/WebApplicationExtensions.cs`

## Technology Reference

- **.NET:** 8.0 (LTS)
- **Database:** SQL Server 2025 (Docker: `mcr.microsoft.com/mssql/server:2025-latest`)
- **ORM:** Entity Framework Core 8.0.21
- **Authentication:** JWT Bearer + ASP.NET Core Identity
- **API Docs:** Swagger/OpenAPI (Swashbuckle 6.6.2)
- **PDF Generation:** QuestPDF 2024.12.0 (Community license)
- **Container:** Docker + Docker Compose

## Additional Notes

- **Nullable reference types enabled** - always handle potential nulls
- **ImplicitUsings enabled** - common namespaces imported globally
- **API versioning:** All endpoints use `/api/v1/` prefix
- **Swagger UI:** Available at `/swagger` when running in Development mode
- **User Secrets:** Configured for WebApi project (ID: `d8fefd4c-683c-4872-b376-b2c0dfe4f175`)
- **SensorsEmulator:** Standalone console app for testing sensor integration (separate .csproj)
