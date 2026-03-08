using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Clubs;

public sealed record RemoveMemberFromClubCommand(Guid ClubId, Guid MemberId);

public sealed class RemoveMemberFromClubHandler(IClubMembershipRepository repo)
{
    public Task<bool> HandleAsync(RemoveMemberFromClubCommand command, CancellationToken ct = default)
        => repo.DeleteAsync(ClubId.From(command.ClubId), MemberId.From(command.MemberId), ct);
}
