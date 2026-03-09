using ClubMonitor.Domain.Members;

namespace ClubMonitor.Domain.Clubs;

public sealed class ClubMembership
{
    public ClubMembershipId Id { get; private set; }
    public ClubId ClubId { get; private set; }
    public MemberId MemberId { get; private set; }
    public ClubRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    private ClubMembership() { }

    public static ClubMembership Create(ClubId clubId, MemberId memberId, ClubRole role)
    {
        return new ClubMembership
        {
            Id = ClubMembershipId.New(),
            ClubId = clubId,
            MemberId = memberId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }
}
