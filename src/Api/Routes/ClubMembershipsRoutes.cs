using ClubMonitor.Application.Clubs;
using ClubMonitor.Domain.Clubs;

namespace Api.Routes;

public static class ClubMembershipsRoutes
{
    public static void MapClubMembershipsRoutes(this WebApplication app)
    {
        var clubsGroup = app.MapGroup("/api/clubs");
        var membersGroup = clubsGroup.MapGroup("{clubId:guid}/members");

        membersGroup.MapPost("", async (Guid clubId, AddMemberToClubBody body, AddMemberToClubHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.HandleAsync(new AddMemberToClubCommand(clubId, body.MemberId, body.Role), ct);
                return Results.Created($"/api/clubs/{clubId}/members", result);
            }
            catch (DuplicateMembershipException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        membersGroup.MapGet("", async (Guid clubId, int? skip, int? take, ListClubMembersHandler handler, CancellationToken ct) =>
        {
            var safeSkip = skip is null or < 0 ? 0 : skip.Value;
            var safeTake = take is null or <= 0 ? 50 : take.Value;
            if (safeTake > 100)
            {
                safeTake = 100;
            }

            var members = await handler.HandleAsync(new ListClubMembersQuery(clubId, safeSkip, safeTake), ct);
            return Results.Ok(members);
        });

        membersGroup.MapDelete("/{memberId:guid}", async (Guid clubId, Guid memberId, RemoveMemberFromClubHandler handler, CancellationToken ct) =>
        {
            var deleted = await handler.HandleAsync(new RemoveMemberFromClubCommand(clubId, memberId), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    record AddMemberToClubBody(Guid MemberId, ClubRole Role);
}

