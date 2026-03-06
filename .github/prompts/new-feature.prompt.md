---
description: "Scaffold a full DDD vertical slice for a new feature: domain types, CQRS handlers, repository interface and implementation, Minimal API endpoints, and integration tests."
agent: "agent"
argument-hint: "Feature name (e.g. Subscription, Event, Venue)"
---

Scaffold a complete DDD vertical slice for the **$ARGUMENTS** feature.

Follow every convention in [copilot-instructions.md](../copilot-instructions.md) exactly. Use the `Members` vertical slice as the reference implementation.

## Steps

### 1. Domain layer — `src/Domain/$ARGUMENTS/`

Create these files:

- **`${ARGUMENTS}Id.cs`** — `readonly record struct` wrapping `Guid` with `static New()` and `static From(Guid)`.
- **`${ARGUMENTS}.cs`** — `sealed class`, private constructor (EF-friendly), `static Create(...)` factory, all properties `private set`.
- **`I${ARGUMENTS}Repository.cs`** — interface with at minimum:
  - `Task<${ARGUMENTS}?> FindByIdAsync(${ARGUMENTS}Id id, CancellationToken ct = default);`
  - `Task AddAsync(${ARGUMENTS} entity, CancellationToken ct = default);`
  - `Task SaveChangesAsync(CancellationToken ct = default);`
- Any value objects or domain exceptions relevant to the feature (same patterns as `Email` / `DuplicateEmailException`).

### 2. Application layer — `src/Application/$ARGUMENTS/`

Create one file per handler. Each file contains the command/query record, result/DTO record, and handler class.

At minimum:
- **`Create${ARGUMENTS}Handler.cs`** — `Create${ARGUMENTS}Command`, `Create${ARGUMENTS}Result`, `Create${ARGUMENTS}Handler`.
- **`Get${ARGUMENTS}ByIdHandler.cs`** — `Get${ARGUMENTS}ByIdQuery`, `${ARGUMENTS}Dto`, `Get${ARGUMENTS}ByIdHandler`.

Register both handlers as `Scoped` in `src/Application/DependencyInjection.cs`.

### 3. Infrastructure layer — `src/Infrastructure/$ARGUMENTS/`

- **`${ARGUMENTS}Repository.cs`** — `internal sealed class ${ARGUMENTS}Repository : I${ARGUMENTS}Repository`, constructor-injected `AppDbContext`.
- Add `DbSet<${ARGUMENTS}> ${ARGUMENTS}s` to `AppDbContext`.
- Configure the entity in `OnModelCreating`: snake_case table name, snake_case columns, any value-object converters and unique indexes.
- Register `services.AddScoped<I${ARGUMENTS}Repository, ${ARGUMENTS}Repository>()` in `src/Infrastructure/DependencyInjection.cs`.

### 4. API endpoints — `src/Api/Program.cs`

Add inside the existing endpoint block:
- `POST /api/${arguments}` → 201 Created / 409 Conflict / 400 Bad Request
- `GET /api/${arguments}/{id:guid}` → 200 OK / 404 Not Found

### 5. EF Core migration

After scaffolding, remind the user to run:
```bash
dotnet ef migrations add Add${ARGUMENTS} --project src/Infrastructure --startup-project src/Api
```

### 6. Integration tests — `tests/Api.IntegrationTests/${ARGUMENTS}EndpointsTests.cs`

Create a `[TestFixture]` class using `TestWebApplicationFactory`. Include:
- A round-trip test: POST then GET returns the expected values.
- A conflict test (if the feature has a uniqueness constraint): duplicate POST returns 409.
- A not-found test: GET with a random Guid returns 404.

Use FluentAssertions. Do **not** use xUnit — this project uses NUnit 4.
