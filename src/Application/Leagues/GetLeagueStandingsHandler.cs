using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Fixtures;
using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record GetLeagueStandingsQuery(Guid LeagueId);

public sealed record StandingDto(
    Guid ClubId,
    string ClubName,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);

public sealed class GetLeagueStandingsHandler(
    ILeagueRepository leagueRepo,
    IFixtureRepository fixtureRepo,
    IClubRepository clubRepo)
{
    public async Task<IReadOnlyList<StandingDto>> HandleAsync(GetLeagueStandingsQuery query, CancellationToken ct = default)
    {
        var leagueId = LeagueId.From(query.LeagueId);
        var entries = await leagueRepo.ListEntriesAsync(leagueId, 0, int.MaxValue, ct);
        var fixtures = await fixtureRepo.ListPlayedByCompetitionAsync(CompetitionType.League, query.LeagueId, ct);

        // Initialise stats for every club in the league
        var stats = entries.ToDictionary(
            e => e.ClubId.Value,
            _ => (played: 0, won: 0, drawn: 0, lost: 0, gf: 0, ga: 0));

        foreach (var f in fixtures)
        {
            if (!f.HomeScore.HasValue || !f.AwayScore.HasValue) continue;

            var homeId = f.HomeClubId.Value;
            var awayId = f.AwayClubId.Value;

            if (!stats.ContainsKey(homeId) || !stats.ContainsKey(awayId)) continue;

            var hs = f.HomeScore.Value;
            var as_ = f.AwayScore.Value;

            var h = stats[homeId];
            var a = stats[awayId];

            h.played++; a.played++;
            h.gf += hs; h.ga += as_;
            a.gf += as_; a.ga += hs;

            if (hs > as_) { h.won++; a.lost++; }
            else if (hs < as_) { h.lost++; a.won++; }
            else { h.drawn++; a.drawn++; }

            stats[homeId] = h;
            stats[awayId] = a;
        }

        // Resolve club names
        var clubNames = new Dictionary<Guid, string>();
        foreach (var entry in entries)
        {
            var club = await clubRepo.FindByIdAsync(entry.ClubId, ct);
            clubNames[entry.ClubId.Value] = club?.Name ?? "Unknown";
        }

        return stats
            .Select(kvp =>
            {
                var (p, w, d, l, gf, ga) = kvp.Value;
                var name = clubNames.TryGetValue(kvp.Key, out var n) ? n : "Unknown";
                return new StandingDto(kvp.Key, name, p, w, d, l, gf, ga, gf - ga, w * 3 + d);
            })
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .ThenBy(s => s.ClubName)
            .ToList();
    }
}
