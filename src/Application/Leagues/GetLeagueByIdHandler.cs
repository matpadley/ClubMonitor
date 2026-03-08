using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record GetLeagueByIdQuery(Guid Id);

public sealed class GetLeagueByIdHandler(ILeagueRepository repo)
{
    public async Task<LeagueDto?> HandleAsync(GetLeagueByIdQuery query, CancellationToken ct = default)
    {
        var league = await repo.FindByIdAsync(LeagueId.From(query.Id), ct);
        if (league is null) return null;
        return new LeagueDto(league.Id.Value, league.Name, league.CreatedAt);
    }
}
