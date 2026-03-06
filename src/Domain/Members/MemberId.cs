namespace ClubMonitor.Domain.Members;

public readonly record struct MemberId(Guid Value)
{
    public static MemberId New() => new(Guid.NewGuid());
    public static MemberId From(Guid value) => new(value);
}
