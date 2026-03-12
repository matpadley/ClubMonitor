using ClubMonitor.Domain.Clubs;
using ClubMonitor.Shared;

namespace ClubMonitor.Application.Clubs;

public sealed record ListClubMembersQuery(Guid ClubId, int Skip, int Take);

public sealed class ListClubMembersHandler(IClubMembershipRepository repo)
{
    public async Task<IReadOnlyList<ClubMembershipDto>> HandleAsync(ListClubMembersQuery query, CancellationToken ct = default)
    {
        var memberships = await repo.ListByClubAsync(ClubId.From(query.ClubId), query.Skip, query.Take, ct);
        return memberships
              .Select(m => new ClubMembershipDto(m.Id.Value, m.ClubId.Value, m.MemberId.Value, m.Role.ToString(), m.JoinedAt))
            .ToList();
    }
}
