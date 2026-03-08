namespace ClubMonitor.Domain.Cups;

public readonly record struct CupEntryId(Guid Value)
{
    public static CupEntryId New() => new(Guid.NewGuid());
    public static CupEntryId From(Guid value) => new(value);
}
