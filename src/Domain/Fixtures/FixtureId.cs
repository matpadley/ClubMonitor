namespace ClubMonitor.Domain.Fixtures;

public readonly record struct FixtureId(Guid Value)
{
    public static FixtureId New() => new(Guid.NewGuid());
    public static FixtureId From(Guid value) => new(value);
}
