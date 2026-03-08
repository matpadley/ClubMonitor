namespace ClubMonitor.Domain.Clubs;

public readonly record struct ClubId(Guid Value)
{
    public static ClubId New() => new(Guid.NewGuid());
    public static ClubId From(Guid value) => new(value);
}
