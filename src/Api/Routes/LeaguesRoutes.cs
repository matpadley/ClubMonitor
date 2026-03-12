using ClubMonitor.Application.Leagues;
using ClubMonitor.Domain.Leagues;

namespace Api.Routes;

public static class LeaguesRoutes
{
    public static void MapLeaguesRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/leagues");

        group.MapPost("", async (CreateLeagueCommand command, CreateLeagueHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/leagues/{result.Id}", result);
            }
            catch (DuplicateLeagueNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetLeagueByIdHandler handler, CancellationToken ct) =>
        {
            var league = await handler.HandleAsync(new GetLeagueByIdQuery(id), ct);
            return league is null ? Results.NotFound() : Results.Ok(league);
        });

        group.MapGet("", async (int? skip, int? take, ListLeaguesHandler handler, CancellationToken ct) =>
        {
            var leagues = await handler.HandleAsync(new ListLeaguesQuery(skip ?? 0, take ?? 50), ct);
            return Results.Ok(leagues);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateLeagueBody body, UpdateLeagueHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new UpdateLeagueCommand(id, body.Name), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (DuplicateLeagueNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, DeleteLeagueHandler handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new DeleteLeagueCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        var clubsGroup = group.MapGroup("{leagueId:guid}/clubs");

        clubsGroup.MapPost("", async (Guid leagueId, AddClubToLeagueBody body, AddClubToLeagueHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new AddClubToLeagueCommand(leagueId, body.ClubId), ct);
                return Results.Created($"/api/leagues/{leagueId}/clubs", result);
            }
            catch (DuplicateLeagueEntryException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        clubsGroup.MapGet("", async (Guid leagueId, int? skip, int? take, ListLeagueClubsHandler handler, CancellationToken ct) =>
        {
            var entries = await handler.HandleAsync(new ListLeagueClubsQuery(leagueId, skip ?? 0, take ?? 50), ct);
            return Results.Ok(entries);
        });

        clubsGroup.MapDelete("/{clubId:guid}", async (Guid leagueId, Guid clubId, RemoveClubFromLeagueHandler handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new RemoveClubFromLeagueCommand(leagueId, clubId), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/{leagueId:guid}/standings", async (Guid leagueId, GetLeagueStandingsHandler handler, CancellationToken ct) =>
        {
            var standings = await handler.HandleAsync(new GetLeagueStandingsQuery(leagueId), ct);
            return Results.Ok(standings);
        });
    }

    record UpdateLeagueBody(string Name);
    record AddClubToLeagueBody(Guid ClubId);
}

