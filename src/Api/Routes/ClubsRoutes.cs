using ClubMonitor.Application.Clubs;
using ClubMonitor.Domain.Clubs;

namespace Api.Routes;

public static class ClubsRoutes
{
    public static void MapClubsRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/clubs");

        group.MapPost("", async (CreateClubCommand command, CreateClubHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/clubs/{result.Id}", result);
            }
            catch (DuplicateClubNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetClubByIdHandler handler, CancellationToken ct) =>
        {
            var club = await handler.HandleAsync(new GetClubByIdQuery(id), ct);
            return club is null ? Results.NotFound() : Results.Ok(club);
        });

        group.MapGet("", async (int? skip, int? take, ListClubsHandler handler, CancellationToken ct) =>
        {
            var clubs = await handler.HandleAsync(new ListClubsQuery(skip ?? 0, take ?? 50), ct);
            return Results.Ok(clubs);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateClubBody body, UpdateClubHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new UpdateClubCommand(id, body.Name), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (DuplicateClubNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, DeleteClubHandler handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new DeleteClubCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    record UpdateClubBody(string Name);
}

