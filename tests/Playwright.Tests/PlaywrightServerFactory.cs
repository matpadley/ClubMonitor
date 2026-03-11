using ClubMonitor.Application;
using ClubMonitor.Application.Members;
using ClubMonitor.Infrastructure;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;

namespace Playwright.Tests;

/// <summary>
/// Starts the full application on a real Kestrel port (random) so Playwright
/// can navigate to it via browser.
///
/// Builds a WebApplication directly rather than using WebApplicationFactory
/// because WebApplicationFactory's EnsureServer() hard-casts IServer to
/// TestServer and throws when Kestrel is in use.
///
/// SQLite in-memory replaces PostgreSQL, following the same three-step EF
/// Core 10 override pattern as Api.IntegrationTests.
/// </summary>
public sealed class PlaywrightServerFactory : IDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _keepAliveConnection;

    public string BaseUrl { get; }

    public PlaywrightServerFactory(string dbName = "PlaywrightTest")
    {
        var connectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";
        _keepAliveConnection = new SqliteConnection(connectionString);
        _keepAliveConnection.Open();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            // MapStaticAssets() looks for <ApplicationName>.staticwebassets.endpoints.json;
            // setting this ensures it finds the Api manifest that ships with the Api.dll.
            ApplicationName = "Api"
        });

        // Satisfy AddInfrastructure's connection-string guard.
        builder.Configuration["ConnectionStrings:ClubMonitor"] = "Host=localhost;Database=test";

        // Port 0 → OS assigns a random free port; read back after Start().
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        // In non-Development environments ASP.NET does not auto-load static web
        // assets (including _framework/blazor.web.js).  Enabling them explicitly
        // ensures the Blazor circuit JS is served from the build-output manifest.
        builder.WebHost.UseStaticWebAssets();

/*
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddMudServices();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
            */

        // Replace Npgsql with SQLite — same three-step override.
        builder.Services.RemoveAll<AppDbContext>();
        builder.Services.RemoveAll<DbContextOptions<AppDbContext>>();
        builder.Services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

        var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .UseInternalServiceProvider(
                new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider())
            .Options;

        builder.Services.AddSingleton(sqliteOptions);
        builder.Services.AddDbContext<AppDbContext>();

        _app = builder.Build();

        _app.UseAntiforgery();
        _app.MapStaticAssets();
        _app.MapRazorComponents<Api.Components.App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Client.Components.Layout.MainLayout).Assembly);

        _app.StartAsync().GetAwaiter().GetResult();

        using (var scope = _app.Services.CreateScope())
        {
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>()
                .Database.EnsureCreated();
        }

        // UseUrls("http://127.0.0.1:0") means the bound port is in app.Urls.
        BaseUrl = _app.Urls.First().TrimEnd('/');
    }

    // --- Seeding helpers used by test fixtures ---

    public async Task<Guid> CreateMemberAsync(string name, string email)
    {
        using var scope = _app.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreateMemberHandler>();
        var result = await handler.HandleAsync(new CreateMemberCommand(name, email));
        return result.Id;
    }

    public void Dispose()
    {
        _app.StopAsync().GetAwaiter().GetResult();
        _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _keepAliveConnection.Dispose();
    }
}
