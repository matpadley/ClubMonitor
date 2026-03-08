using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record RemoveClubFromLeagueCommand(Guid LeagueId, Guid ClubId);

public sealed class RemoveClubFromLeagueHandler(ILeagueRepository repo)
{
    public Task<bool> HandleAsync(RemoveClubFromLeagueCommand command, CancellationToken ct = default)
        => repo.RemoveEntryAsync(LeagueId.From(command.LeagueId), ClubId.From(command.ClubId), ct);
}
