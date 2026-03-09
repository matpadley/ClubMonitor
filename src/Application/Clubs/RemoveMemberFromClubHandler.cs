using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Clubs;

public sealed record RemoveMemberFromClubCommand(Guid ClubId, Guid MemberId);

public sealed class RemoveMemberFromClubHandler(IClubMembershipRepository repo)
{
    public async Task<bool> HandleAsync(RemoveMemberFromClubCommand command, CancellationToken ct = default)
    {
        var deleted = await repo.DeleteAsync(ClubId.From(command.ClubId), MemberId.From(command.MemberId), ct);
        if (!deleted) return false;
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
