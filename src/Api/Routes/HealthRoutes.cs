using ClubMonitor.Infrastructure.Persistence;

namespace Api.Routes;

public static class HealthRoutes
{
    public static void MapHealthRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/db/ping", async (AppDbContext db) =>
        {
            var canConnect = await db.Database.CanConnectAsync();
            return Results.Ok(new { canConnect });
        });

        group.MapGet("/health", () => Results.Ok(new { status = "ok" }));
    }
}

