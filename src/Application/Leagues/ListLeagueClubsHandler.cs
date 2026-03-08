using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record ListLeagueClubsQuery(Guid LeagueId, int Skip, int Take);

public sealed class ListLeagueClubsHandler(ILeagueRepository repo)
{
    public async Task<IReadOnlyList<LeagueEntryDto>> HandleAsync(ListLeagueClubsQuery query, CancellationToken ct = default)
    {
        var entries = await repo.ListEntriesAsync(LeagueId.From(query.LeagueId), query.Skip, query.Take, ct);
        return entries
            .Select(e => new LeagueEntryDto(e.Id.Value, e.LeagueId.Value, e.ClubId.Value, e.EnteredAt))
            .ToList();
    }
}
