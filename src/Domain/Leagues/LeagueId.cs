namespace ClubMonitor.Domain.Leagues;

public readonly record struct LeagueId(Guid Value)
{
    public static LeagueId New() => new(Guid.NewGuid());
    public static LeagueId From(Guid value) => new(value);
}
