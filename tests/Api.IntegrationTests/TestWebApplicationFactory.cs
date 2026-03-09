using ClubMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // A single open connection keeps the named shared-cache in-memory database alive
    // for the entire factory lifetime. All EF Core connections (via EnsureCreated and
    // request scopes) share the same named database through the shared cache.
    private readonly string _connectionString;
    private readonly SqliteConnection _keepAliveConnection;

    public TestWebApplicationFactory(string dbName = "ClubMonitorTest")
    {
        _connectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide a dummy connection string so AddInfrastructure's guard check passes
        builder.UseSetting("ConnectionStrings:ClubMonitor", "Host=localhost;Database=test");
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // EF Core 10 stores each AddDbContext optionsAction as
            // IDbContextOptionsConfiguration<TContext>. Removing only AppDbContext
            // or DbContextOptions<AppDbContext> is not enough — the Npgsql config
            // bleeds through. Remove all three descriptor types, then register a
            // pre-built SQLite options singleton that bypasses the pipeline entirely.
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connectionString)
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider())
                .Options;

            services.AddSingleton(sqliteOptions);
            services.AddDbContext<AppDbContext>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _keepAliveConnection.Dispose();
    }
}