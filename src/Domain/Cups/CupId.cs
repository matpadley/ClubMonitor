namespace ClubMonitor.Domain.Cups;

public readonly record struct CupId(Guid Value)
{
    public static CupId New() => new(Guid.NewGuid());
    public static CupId From(Guid value) => new(value);
}
