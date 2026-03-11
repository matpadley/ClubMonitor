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
| Clubs | Manage clubs and their member rosters |
| Leagues | League management with club entries and standings |
| Cups | Cup competition management with draw functionality |
| Fixtures | Fixture scheduling and result recording |

## API Endpoints

### Members
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/members` | Create a member |
| `GET` | `/api/members` | List members (paginated: `skip`, `take`) |
| `GET` | `/api/members/{id}` | Get member by ID |
| `PUT` | `/api/members/{id}` | Update member |
| `DELETE` | `/api/members/{id}` | Delete member |

### Clubs
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/clubs` | Create a club |
| `GET` | `/api/clubs` | List clubs (paginated) |
| `GET` | `/api/clubs/{id}` | Get club by ID |
| `PUT` | `/api/clubs/{id}` | Update club |
| `DELETE` | `/api/clubs/{id}` | Delete club |
| `POST` | `/api/clubs/{id}/members` | Add member to club |
| `GET` | `/api/clubs/{id}/members` | List club members (paginated) |
| `DELETE` | `/api/clubs/{id}/members/{memberId}` | Remove member from club |

### Leagues
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/leagues` | Create a league |
| `GET` | `/api/leagues` | List leagues (paginated) |
| `GET` | `/api/leagues/{id}` | Get league by ID |
| `PUT` | `/api/leagues/{id}` | Update league |
| `DELETE` | `/api/leagues/{id}` | Delete league |
| `POST` | `/api/leagues/{id}/clubs` | Add club to league |
| `GET` | `/api/leagues/{id}/clubs` | List league clubs (paginated) |
| `DELETE` | `/api/leagues/{id}/clubs/{clubId}` | Remove club from league |
| `GET` | `/api/leagues/{id}/standings` | Get league standings |

### Cups
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/cups` | Create a cup |
| `GET` | `/api/cups` | List cups (paginated) |
| `GET` | `/api/cups/{id}` | Get cup by ID |
| `PUT` | `/api/cups/{id}` | Update cup |
| `DELETE` | `/api/cups/{id}` | Delete cup |
| `POST` | `/api/cups/{id}/clubs` | Add club to cup |
| `GET` | `/api/cups/{id}/clubs` | List cup clubs (paginated) |
| `DELETE` | `/api/cups/{id}/clubs/{clubId}` | Remove club from cup |
| `POST` | `/api/cups/{id}/draw` | Perform cup draw |

### Fixtures
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/fixtures` | Create a fixture |
| `GET` | `/api/fixtures/{id}` | Get fixture by ID |
| `GET` | `/api/fixtures?competitionId=...` | List fixtures by competition |
| `PUT` | `/api/fixtures/{id}/result` | Record a result |
| `PUT` | `/api/fixtures/{id}/reschedule` | Reschedule a fixture |

### Diagnostics
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/health` | Health check |
| `GET` | `/api/db/ping` | Database connectivity check |

## Build and Run

```bash
# Start PostgreSQL
docker-compose up -d

# Restore and build
dotnet restore
dotnet build

# Run the API (http://localhost:5000, Swagger UI at /swagger)
dotnet run --project src/Api/Api.csproj
```

### EF Core Migrations

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## Testing

### Integration Tests (NUnit 4 + SQLite in-memory)

```bash
dotnet test tests/Api.IntegrationTests/Api.IntegrationTests.csproj
```

Tests use `WebApplicationFactory<Program>` with SQLite replacing PostgreSQL. See `tests/Api.IntegrationTests/TestWebApplicationFactory.cs` for the setup pattern.

### Playwright End-to-End Tests

```bash
# Install Playwright browsers (first time only)
pwsh tests/Playwright.Tests/bin/Debug/net10.0/playwright.ps1 install

dotnet test tests/Playwright.Tests/Playwright.Tests.csproj
```

Playwright tests spin up a real Kestrel instance (`PlaywrightServerFactory`) with SQLite in-memory and drive a browser against the Blazor UI.

## Solution Format

The solution uses the `.slnx` format (`ClubMonitor.slnx`) — open with Visual Studio 2022 17.10+ or the `dotnet` CLI.