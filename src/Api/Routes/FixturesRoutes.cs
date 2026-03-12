using ClubMonitor.Application.Fixtures;
using ClubMonitor.Domain.Fixtures;

namespace Api.Routes;

public static class FixturesRoutes
{
    public static void MapFixturesRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/fixtures");

        group.MapPost("", async (CreateFixtureCommand command, CreateFixtureHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/fixtures/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetFixtureByIdHandler handler, CancellationToken ct) =>
        {
            var fixture = await handler.HandleAsync(new GetFixtureByIdQuery(id), ct);
            return fixture is null ? Results.NotFound() : Results.Ok(fixture);
        });

        group.MapGet("", async (string? type, Guid? competitionId, int? skip, int? take, ListFixturesByCompetitionHandler handler, CancellationToken ct) =>
        {
            if (!Enum.TryParse<CompetitionType>(type, ignoreCase: true, out var competitionType))
                return Results.BadRequest(new { error = "Invalid competition type. Use 'League' or 'Cup'." });
            if (competitionId is null)
                return Results.BadRequest(new { error = "competitionId is required." });

            var effectiveSkip = skip ?? 0;
            var effectiveTake = take ?? 50;

            if (effectiveSkip < 0)
                return Results.BadRequest(new { error = "skip must be greater than or equal to 0." });
            if (effectiveTake <= 0)
                return Results.BadRequest(new { error = "take must be greater than 0." });

            const int MaxPageSize = 100;
            if (effectiveTake > MaxPageSize)
                effectiveTake = MaxPageSize;

            var fixtures = await handler.HandleAsync(new ListFixturesByCompetitionQuery(competitionType, competitionId.Value, effectiveSkip, effectiveTake), ct);
            return Results.Ok(fixtures);
        });

        group.MapPut("/{id:guid}/result", async (Guid id, RecordResultBody body, RecordResultHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new RecordResultCommand(id, body.HomeScore, body.AwayScore), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPut("/{id:guid}/schedule", async (Guid id, RescheduleBody body, RescheduleFixtureHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new RescheduleFixtureCommand(id, body.ScheduledAt, body.Venue), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Static assets mapped from a small helper
        app.MapStaticAssets();
    }

    record RecordResultBody(int HomeScore, int AwayScore);
    record RescheduleBody(DateTimeOffset ScheduledAt, string? Venue);
}

