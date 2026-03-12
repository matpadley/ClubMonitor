using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Clubs;

public sealed record AddMemberToClubCommand(Guid ClubId, Guid MemberId, ClubRole Role);


public sealed class AddMemberToClubHandler(
    IClubRepository clubRepo,
    IMemberRepository memberRepo,
    IClubMembershipRepository membershipRepo)
{
    public async Task<ClubMonitor.Shared.ClubMembershipDto> HandleAsync(AddMemberToClubCommand command, CancellationToken ct = default)
    {
        var clubId = ClubId.From(command.ClubId);
        var memberId = MemberId.From(command.MemberId);

        var club = await clubRepo.FindByIdAsync(clubId, ct);
        if (club is null)
            throw new ArgumentException($"Club '{command.ClubId}' not found.", nameof(command.ClubId));

        var member = await memberRepo.FindByIdAsync(memberId, ct);
        if (member is null)
            throw new ArgumentException($"Member '{command.MemberId}' not found.", nameof(command.MemberId));

        if (await membershipRepo.ExistsAsync(clubId, memberId, ct))
            throw new DuplicateMembershipException(command.ClubId, command.MemberId);

        var membership = ClubMembership.Create(clubId, memberId, command.Role);
        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);

        return new ClubMonitor.Shared.ClubMembershipDto(membership.Id.Value, membership.ClubId.Value, membership.MemberId.Value, membership.Role.ToString(), membership.JoinedAt);
    }
}
