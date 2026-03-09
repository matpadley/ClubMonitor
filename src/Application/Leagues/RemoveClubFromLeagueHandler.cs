using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record RemoveClubFromLeagueCommand(Guid LeagueId, Guid ClubId);

public sealed class RemoveClubFromLeagueHandler(ILeagueRepository repo)
{
    public async Task<bool> HandleAsync(RemoveClubFromLeagueCommand command, CancellationToken ct = default)
    {
        var removed = await repo.RemoveEntryAsync(LeagueId.From(command.LeagueId), ClubId.From(command.ClubId), ct);
        if (!removed) return false;
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
