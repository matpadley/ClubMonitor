using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record DeleteClubCommand(Guid Id);

public sealed class DeleteClubHandler(IClubRepository repo)
{
    public async Task<bool> HandleAsync(DeleteClubCommand command, CancellationToken ct = default)
    {
        var deleted = await repo.DeleteAsync(ClubId.From(command.Id), ct);
        if (!deleted) return false;
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
