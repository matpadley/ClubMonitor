using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record ListClubsQuery(int Skip, int Take);

public sealed class ListClubsHandler(IClubRepository repo)
{
    public async Task<IReadOnlyList<ClubMonitor.Shared.ClubDto>> HandleAsync(ListClubsQuery query, CancellationToken ct = default)
    {
        var clubs = await repo.ListAsync(query.Skip, query.Take, ct);
        return clubs.Select(c => new ClubMonitor.Shared.ClubDto(c.Id.Value, c.Name, c.CreatedAt)).ToList();
    }
}
