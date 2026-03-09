using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record DeleteLeagueCommand(Guid Id);

public sealed class DeleteLeagueHandler(ILeagueRepository repo)
{
    public async Task<bool> HandleAsync(DeleteLeagueCommand command, CancellationToken ct = default)
    {
        var deleted = await repo.DeleteAsync(LeagueId.From(command.Id), ct);
        if (!deleted) return false;
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
