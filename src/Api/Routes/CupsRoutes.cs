using ClubMonitor.Application.Cups;
using ClubMonitor.Domain.Cups;

namespace Api.Routes;

public static class CupsRoutes
{
    public static void MapCupsRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/cups");

        group.MapPost("", async (CreateCupCommand command, CreateCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/cups/{result.Id}", result);
            }
            catch (DuplicateCupNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetCupByIdHandler handler, CancellationToken ct) =>
        {
            var cup = await handler.HandleAsync(new GetCupByIdQuery(id), ct);
            return cup is null ? Results.NotFound() : Results.Ok(cup);
        });

        group.MapGet("", async (int? skip, int? take, ListCupsHandler handler, CancellationToken ct) =>
        {
            var cups = await handler.HandleAsync(new ListCupsQuery(skip ?? 0, take ?? 50), ct);
            return Results.Ok(cups);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCupBody body, UpdateCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new UpdateCupCommand(id, body.Name), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (DuplicateCupNameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, DeleteCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var deleted = await handler.HandleAsync(new DeleteCupCommand(id), ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidCupStateException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        var clubsGroup = group.MapGroup("{cupId:guid}/clubs");

        clubsGroup.MapPost("", async (Guid cupId, AddClubToCupBody body, AddClubToCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new AddClubToCupCommand(cupId, body.ClubId), ct);
                return Results.Created($"/api/cups/{cupId}/clubs", result);
            }
            catch (DuplicateCupEntryException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (InvalidCupStateException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        clubsGroup.MapGet("", async (Guid cupId, int? skip, int? take, ListCupClubsHandler handler, CancellationToken ct) =>
        {
            var entries = await handler.HandleAsync(new ListCupClubsQuery(cupId, skip ?? 0, take ?? 50), ct);
            return Results.Ok(entries);
        });

        clubsGroup.MapDelete("/{clubId:guid}", async (Guid cupId, Guid clubId, RemoveClubFromCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var deleted = await handler.HandleAsync(new RemoveClubFromCupCommand(cupId, clubId), ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (InvalidCupStateException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPost("/{cupId:guid}/draw", async (Guid cupId, DrawCupHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new DrawCupCommand(cupId), ct);
                return Results.Ok(result);
            }
            catch (InvalidCupStateException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    record UpdateCupBody(string Name);
    record AddClubToCupBody(Guid ClubId);
}

