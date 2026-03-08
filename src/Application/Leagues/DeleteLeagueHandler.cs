using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record DeleteLeagueCommand(Guid Id);

public sealed class DeleteLeagueHandler(ILeagueRepository repo)
{
    public Task<bool> HandleAsync(DeleteLeagueCommand command, CancellationToken ct = default)
        => repo.DeleteAsync(LeagueId.From(command.Id), ct);
}
