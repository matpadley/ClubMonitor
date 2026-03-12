# ClubMonitor

A .NET 10 solution for managing sports clubs, leagues, cups, fixtures, and members. Built with Domain-Driven Design, lightweight CQRS, Minimal API, Blazor SSR, and EF Core with PostgreSQL.

## Architecture

Four-layer DDD structure: **Domain → Application → Infrastructure → Api**

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `src/Domain` | Entities, value objects, repository interfaces, domain exceptions. Zero framework dependencies. |
| Application | `src/Application` | Command/query handlers. References only Domain. No EF Core or HTTP. |
| Infrastructure | `src/Infrastructure` | EF Core `AppDbContext`, repositories, PostgreSQL setup. |
| Api | `src/Api` | Minimal API endpoints, Blazor SSR (`src/Client`), DI wiring. |

## Features

| Feature | Description |
|---------|-------------|
| Members | Create, read, update, delete members with email uniqueness enforcement |
# ClubMonitor

A lightweight sports competition manager built with .NET 10 using Domain-Driven Design (DDD), a minimal CQRS approach, Minimal APIs, Blazor Server-side rendering (SSR), and EF Core with PostgreSQL.

> [!TIP]
> The repository follows a strict four-layer DDD structure: Domain → Application → Infrastructure → Api. This keeps domain logic isolated and testable.

## Table of contents

- [Architecture & Conventions](#architecture--conventions)
- [Key Features](#key-features)
- [Quick start](#quick-start)
- [Build & run](#build--run)
- [EF Core migrations](#ef-core-migrations)
- [Testing](#testing)
- [Useful notes](#useful-notes)

## Architecture & conventions

Project layout (top-level folders under `src/`):

- `Domain` — Entities, value objects, repository interfaces, and domain exceptions. No framework dependencies.
- `Application` — Commands/queries and their handlers (lightweight CQRS). References only `Domain`.
- `Infrastructure` — EF Core `AppDbContext`, repository implementations, and persistence concerns.
- `Api` — Minimal API endpoints, DI wiring, and the Blazor `Client` project reference for SSR UI.

Conventions you should know:

- Entities use private constructors + `static Create(...)` factory methods.
- Value objects are `sealed` with private constructors and `static Create(...)` factories and implement `Equals`/`GetHashCode`.
- Strongly-typed IDs are `readonly record struct` wrappers around `Guid` with `New()` / `From(Guid)` helpers.
- Handlers are plain classes in `src/Application/{Feature}/...` with a command/query record, a result DTO record, and a handler class. No MediatR.
- Repository interfaces live in `Domain`; EF Core-backed implementations live in `Infrastructure`.
- Minimal APIs are defined inline in `src/Api/Program.cs` using `MapGet` / `MapPost` etc.
- Database naming: snake_case for tables/columns (configured in `AppDbContext`).

## Key features

- Members: create, list, update, delete, unique-email enforcement
- Clubs: create clubs and manage club memberships
- Leagues & Cups: manage competitions, add/remove clubs, cup draw, league standings
- Fixtures: schedule fixtures, record results, reschedule

API highlights (representative):

- POST /api/members — create member
- GET /api/members — list members (supports `skip`/`take`)
- POST /api/clubs — create club
- POST /api/cups/{id}/draw — run cup draw
- GET /api/leagues/{id}/standings — league standings

See `src/Api/Program.cs` for the full set of routes.

## Quick start

Prerequisites:

- .NET 10 SDK
- Docker (for PostgreSQL during local development) — optional if you use a manually provisioned DB

Start PostgreSQL via Docker Compose (recommended):

```bash
docker-compose up -d
```

Restore and build the solution:

```bash
dotnet restore
dotnet build
```

Run the API (launches on http://localhost:5000 by default):

```bash
dotnet run --project src/Api/Api.csproj
```

When the API is running you can visit `/swagger` for the OpenAPI UI (if enabled) and use the Blazor SSR UI served from the API project.

## Build & run (developer tips)

- The API references the `Client` project for Blazor SSR — building `src/Api` will also build `src/Client`.
- Connection string key: `ConnectionStrings:ClubMonitor` (in `src/Api/appsettings.json`).

## EF Core migrations

Create a migration (requires a reachable PostgreSQL instance or valid connection string):

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

Notes:

- Migrations are defined in the `Infrastructure` project. The runtime startup project is `src/Api`.
- Tables and columns are mapped to `snake_case` in `AppDbContext.OnModelCreating`.

## Testing

Integration and E2E tests are provided.

- Integration tests (NUnit 4 + FluentAssertions) use an in-memory SQLite replacement for PostgreSQL. Run:

```bash
dotnet test tests/Api.IntegrationTests/Api.IntegrationTests.csproj
```

- Playwright end-to-end tests run a real Kestrel instance and drive the Blazor UI. First-time setup requires installing Playwright browsers (powershell script included in the repo). Example:

```bash
# from a PowerShell prompt (Windows/macOS via pwsh)
pwsh tests/Playwright.Tests/bin/Debug/net10.0/playwright.ps1 install
dotnet test tests/Playwright.Tests/Playwright.Tests.csproj
```

Testing notes (important):

> [!WARNING]
> When overriding EF Core services for tests you must replace three service descriptors: `AppDbContext`, `DbContextOptions<AppDbContext>`, and `IDbContextOptionsConfiguration<AppDbContext>` and register pre-built SQLite options with `UseInternalServiceProvider(...)`. See `tests/Api.IntegrationTests/TestWebApplicationFactory.cs` for the canonical pattern.

## Useful notes

- Solution format: the repository uses the `.slnx` solution file (`ClubMonitor.slnx`). Open with `dotnet` or Visual Studio 2022+.
- The test project exposes `Program` for WebApplicationFactory via `public partial class Program { }` — see `src/Api/Program.cs`.
- If you add EF Core providers or change DB providers, be mindful of internal service provider usage in tests to avoid multi-provider conflicts.

If you'd like, I can also produce a shorter quick-reference cheat sheet (commands + common troubleshooting) or add badges for CI and coverage — tell me which you'd prefer.

