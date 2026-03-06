# ClubMonitor — Copilot Instructions

.NET 10 solution using DDD, lightweight CQRS, Minimal API, Blazor SSR, and EF Core with PostgreSQL.

## Architecture

Four-layer DDD structure: **Domain → Application → Infrastructure → Api**

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `src/Domain` | Entities, value objects, repository interfaces, domain exceptions. Zero framework dependencies. |
| Application | `src/Application` | Command/query handlers. References only Domain. No EF Core or HTTP. |
| Infrastructure | `src/Infrastructure` | EF Core `AppDbContext`, `MemberRepository`, PostgreSQL setup. |
| Api | `src/Api` | Minimal API endpoints, Blazor SSR (`Client` project), DI wiring. |

`Directory.Build.props` sets `net10.0`, `<Nullable>enable</Nullable>`, and `<ImplicitUsings>enable</ImplicitUsings>` for all projects.

## Conventions

### Domain objects
- **Entities** use private constructors + a `static Create(...)` factory method. Example: `Member.Create(name, email)`.
- **Value objects** are `sealed class` with private constructor + `static Create(string)`. Implement `Equals`/`GetHashCode` manually. Example: `Email`.
- **Strongly-typed IDs** are `readonly record struct` wrapping `Guid` with `static New()` and `static From(Guid)` helpers. Example: `MemberId`.
- **Domain exceptions** are `sealed class` using primary constructor syntax, inheriting `Exception`. Example: `DuplicateEmailException`.

### CQRS handlers (no MediatR)
Each feature file (`src/Application/{Feature}/{Name}Handler.cs`) contains three types:
1. The command or query record (e.g. `sealed record CreateMemberCommand(...)`)
2. The result/DTO record (e.g. `sealed record CreateMemberResult(...)`)
3. The handler class (e.g. `sealed class CreateMemberHandler`)

Handlers are injected via constructor and registered as `Scoped` in `Application/DependencyInjection.cs`.

### Repository pattern
- Interface lives in Domain: `IMemberRepository` in `src/Domain/Members/`.
- Implementation is `internal sealed class` in Infrastructure: `MemberRepository` in `src/Infrastructure/Members/`.
- Unit-of-work is explicit — callers must call `SaveChangesAsync` after mutations.

### Minimal API endpoints
All HTTP routes are defined inline in `src/Api/Program.cs` using `MapGet`/`MapPost`. No MVC controllers.

### EF Core persistence
- Table and column names use `snake_case` (e.g. table `"members"`, column `"created_at"`).
- Value object and ID conversions are configured in `AppDbContext.OnModelCreating`.
- Connection string key: `ConnectionStrings:ClubMonitor`.

## Build and Test

```bash
# Restore + build
dotnet restore
dotnet build

# Run the API (starts on http://localhost:5000)
dotnet run --project src/Api/Api.csproj

# Run integration tests
dotnet test tests/Api.IntegrationTests/Api.IntegrationTests.csproj
```

EF Core migrations (requires running PostgreSQL):
```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

Docker Compose runs PostgreSQL:
```bash
docker-compose up -d
```

## Testing

- **Framework:** NUnit 4 (`[TestFixture]`, `[Test]`, `[OneTimeSetUp]`/`[OneTimeTearDown]`). **Not xUnit** — do not add `using Xunit;`.
- **Assertions:** FluentAssertions v7.
- **HTTP:** `WebApplicationFactory<Program>` from `Microsoft.AspNetCore.Mvc.Testing`.
- **Database:** SQLite in-memory replaces PostgreSQL in tests.
- `Program` is exposed to the test project via `public partial class Program { }` at the bottom of `Program.cs`.

### EF Core test override (critical)
EF Core 10 stores options as `IDbContextOptionsConfiguration<TContext>`. When overriding the DB in `ConfigureTestServices`, remove **all three** service descriptors — `AppDbContext`, `DbContextOptions<AppDbContext>`, and `IDbContextOptionsConfiguration<AppDbContext>` — then register the SQLite options as a pre-built singleton. Also call `UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider())` to avoid multi-provider conflicts. See `tests/Api.IntegrationTests/TestWebApplicationFactory.cs` for the canonical pattern.

## Project-Specific Notes

- Solution uses the newer `.slnx` format (`ClubMonitor.slnx`), not the classic `.sln`.
- `src/Application/Application.csproj` explicitly references `Microsoft.Extensions.DependencyInjection.Abstractions` v10 — it is not included automatically in a plain SDK project.
- `src/Client/Client.csproj` requires `<FrameworkReference Include="Microsoft.AspNetCore.App" />` for Razor component namespaces.
- `src/Api/Api.csproj` references `src/Client`, so a clean API build always compiles the Blazor client first.
