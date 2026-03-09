namespace ClubMonitor.Domain.Leagues;

public readonly record struct LeagueEntryId(Guid Value)
{
    public static LeagueEntryId New() => new(Guid.NewGuid());
    public static LeagueEntryId From(Guid value) => new(value);
}
