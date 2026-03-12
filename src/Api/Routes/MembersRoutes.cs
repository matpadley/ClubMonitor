using ClubMonitor.Application.Members;
using ClubMonitor.Domain.Members;

namespace Api.Routes;

public static class MembersRoutes
{
    public static void MapMembersRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/members");

        group.MapPost("", async (CreateMemberCommand command, CreateMemberHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/members/{result.Id}", result);
            }
            catch (DuplicateEmailException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetMemberByIdHandler handler, CancellationToken ct) =>
        {
            var member = await handler.HandleAsync(new GetMemberByIdQuery(id), ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        });

        group.MapGet("", async (int? skip, int? take, ListMembersHandler handler, CancellationToken ct) =>
        {
            const int MaxPageSize = 1000;

            var validatedSkip = skip ?? 0;
            var validatedTake = take ?? 50;

            if (validatedSkip < 0)
                return Results.BadRequest(new { error = "skip must be greater than or equal to 0." });
            if (validatedTake <= 0)
                return Results.BadRequest(new { error = "take must be greater than 0." });
            if (validatedTake > MaxPageSize)
                return Results.BadRequest(new { error = $"take cannot be greater than {MaxPageSize}." });

            var members = await handler.HandleAsync(new ListMembersQuery(validatedSkip, validatedTake), ct);
            return Results.Ok(members);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateMemberBody body, UpdateMemberHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new UpdateMemberCommand(id, body.Name, body.Email), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (DuplicateEmailException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, DeleteMemberHandler handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new DeleteMemberCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    record UpdateMemberBody(string Name, string Email);
}

