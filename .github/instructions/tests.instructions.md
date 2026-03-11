---
description: "Use when writing, editing, or reviewing integration tests. Covers NUnit 4 patterns, FluentAssertions style, SQLite in-memory setup, and the EF Core 10 three-descriptor override pattern."
applyTo: "tests/**"
---

## Test Framework

This project uses **NUnit 4**, not xUnit. Never add `using Xunit;` or xUnit attributes.

| Concept | NUnit syntax |
|---------|-------------|
| Test class | `[TestFixture]` |
| Test method | `[Test]` |
| Async test | `[Test] public async Task Name()` |
| Once per class setup | `[OneTimeSetUp]` / `[OneTimeTearDown]` |
| Per-test setup | `[SetUp]` / `[TearDown]` |

## Assertions

Use **FluentAssertions v7**: `actual.Should().Be(expected)`, `response.StatusCode.Should().Be(HttpStatusCode.Created)`, etc. Do not use `Assert.That` or `Assert.AreEqual`.

## HTTP Testing

Use `WebApplicationFactory<Program>` from `Microsoft.AspNetCore.Mvc.Testing`:
- Create the factory in `[OneTimeSetUp]`, dispose in `[OneTimeTearDown]`.
- Create a single `HttpClient` per fixture with `factory.CreateClient()`.

## Database Override (EF Core 10 — critical)

EF Core 10 registers `AddDbContext` options as `IDbContextOptionsConfiguration<TContext>` in the service collection. A naive `RemoveAll<DbContextOptions<AppDbContext>>` is **not enough** — Npgsql options bleed through.

In `ConfigureTestServices`, remove **all three** descriptors:

```csharp
services.RemoveAll<AppDbContext>();
services.RemoveAll<DbContextOptions<AppDbContext>>();
services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
```

Then register SQLite options as a **pre-built singleton** (not a factory) and use `UseInternalServiceProvider` to avoid the multi-provider conflict:

```csharp
private const string ConnectionString = "DataSource=ClubMonitorTest;Mode=Memory;Cache=Shared";
// Keep one connection open for the factory lifetime so the named in-memory DB persists
private readonly SqliteConnection _keepAliveConnection = new(ConnectionString);

var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(ConnectionString)
    .UseInternalServiceProvider(
        new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .BuildServiceProvider())
    .Options;

services.AddSingleton(sqliteOptions);
services.AddDbContext<AppDbContext>();
```

> **Why a named database + keep-alive connection?** SQLite in-memory databases (even with `Cache=Shared`) are destroyed when the last connection to them closes. Without an open connection held by the factory, the schema created by `EnsureCreatedAsync` is gone by the time the first request arrives. The `_keepAliveConnection` keeps the database alive for the entire fixture lifetime.

## Database Initialisation

After the test host is built, ensure the schema is created before any test runs:

```csharp
using var scope = _factory.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.EnsureCreatedAsync();
```

## Program Visibility

`Program` is exposed to the test project via `public partial class Program { }` at the bottom of `src/Api/Program.cs`. Do not remove this.

## Playwright End-to-End Tests

Playwright tests live in `tests/Playwright.Tests/`. They use `PlaywrightServerFactory` (not `WebApplicationFactory`) to start a real Kestrel instance on a random port with SQLite in-memory replacing PostgreSQL — following the same three-descriptor EF Core 10 override pattern as the integration tests.

Key differences from integration tests:

| Aspect | Integration Tests | Playwright Tests |
|--------|-----------------|-----------------|
| Host | `WebApplicationFactory` (TestServer) | `PlaywrightServerFactory` (real Kestrel) |
| Client | `HttpClient` | Playwright `IPage` (browser) |
| Coverage | HTTP API layer | Blazor UI layer |
| Teardown | Factory dispose | `PlaywrightServerFactory.Dispose()` |

`PlaywrightServerFactory` must call `builder.WebHost.UseStaticWebAssets()` so Blazor's `_framework/blazor.web.js` is served in the `Testing` environment.

Install browsers once before running: `pwsh tests/Playwright.Tests/bin/Debug/net10.0/playwright.ps1 install`
