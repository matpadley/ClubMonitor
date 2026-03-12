using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Clubs;

public sealed record ListMemberClubsQuery(Guid MemberId, int Skip, int Take);

public sealed class ListMemberClubsHandler(IClubMembershipRepository repo, IClubRepository clubRepo)
{
    public async Task<IReadOnlyList<ClubDto>> HandleAsync(ListMemberClubsQuery query, CancellationToken ct = default)
    {
        var memberships = await repo.ListByMemberAsync(MemberId.From(query.MemberId), query.Skip, query.Take, ct);
        var clubIds = memberships.Select(m => m.ClubId).ToHashSet();
        var clubs = await clubRepo.ListAsync(0, 1000, ct);
        return clubs.Where(c => clubIds.Contains(c.Id)).Select(c => new ClubDto(c.Id.Value, c.Name, c.CreatedAt)).ToList();
    }
}
