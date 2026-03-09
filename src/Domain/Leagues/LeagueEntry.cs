using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Domain.Leagues;

public sealed class LeagueEntry
{
    public LeagueEntryId Id { get; private set; }
    public LeagueId LeagueId { get; private set; }
    public ClubId ClubId { get; private set; }
    public DateTimeOffset EnteredAt { get; private set; }

    private LeagueEntry() { }

    public static LeagueEntry Create(LeagueId leagueId, ClubId clubId)
    {
        return new LeagueEntry
        {
            Id = LeagueEntryId.New(),
            LeagueId = leagueId,
            ClubId = clubId,
            EnteredAt = DateTimeOffset.UtcNow
        };
    }
}
