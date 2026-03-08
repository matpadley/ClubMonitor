namespace ClubMonitor.Domain.Clubs;

public readonly record struct ClubMembershipId(Guid Value)
{
    public static ClubMembershipId New() => new(Guid.NewGuid());
    public static ClubMembershipId From(Guid value) => new(value);
}
