using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record ListLeaguesQuery(int Skip, int Take);

public sealed class ListLeaguesHandler(ILeagueRepository repo)
{
    public async Task<IReadOnlyList<LeagueDto>> HandleAsync(ListLeaguesQuery query, CancellationToken ct = default)
    {
        var leagues = await repo.ListAsync(query.Skip, query.Take, ct);
        return leagues.Select(l => new LeagueDto(l.Id.Value, l.Name, l.CreatedAt)).ToList();
    }
}
