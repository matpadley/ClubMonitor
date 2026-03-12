using ClubMonitor.Application.UserProfiles;
using ClubMonitor.Domain.UserProfiles;

namespace Api.Routes;

public static class UserProfilesRoutes
{
    public static void MapUserProfilesRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        group.MapPost("", async (RegisterUserCommand command, RegisterUserHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/users/{result.Id}", result);
            }
            catch (DuplicateUsernameException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (DuplicateUserEmailException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, GetUserProfileByIdHandler handler, CancellationToken ct) =>
        {
            var profile = await handler.HandleAsync(new GetUserProfileByIdQuery(id), ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserProfileBody body, UpdateUserProfileHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new UpdateUserProfileCommand(id, body.DisplayName, body.Bio), ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    record UpdateUserProfileBody(string DisplayName, string? Bio);
}

