using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record DeleteMemberCommand(Guid Id);

public sealed class DeleteMemberHandler(IMemberRepository repo)
{
    public async Task<bool> HandleAsync(DeleteMemberCommand command, CancellationToken ct = default)
    {
        var deleted = await repo.DeleteAsync(MemberId.From(command.Id), ct);
        if (!deleted) return false;
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
