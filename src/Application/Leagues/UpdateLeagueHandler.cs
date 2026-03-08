using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record UpdateLeagueCommand(Guid Id, string Name);

public sealed class UpdateLeagueHandler(ILeagueRepository repo)
{
    public async Task<LeagueDto?> HandleAsync(UpdateLeagueCommand command, CancellationToken ct = default)
    {
        var league = await repo.FindByIdAsync(LeagueId.From(command.Id), ct);
        if (league is null) return null;

        var newName = command.Name.Trim();
        if (!string.Equals(league.Name, newName, StringComparison.OrdinalIgnoreCase))
        {
            if (await repo.ExistsWithNameAsync(newName, ct))
                throw new DuplicateLeagueNameException(newName);
        }

        league.Rename(newName);
        await repo.SaveChangesAsync(ct);

        return new LeagueDto(league.Id.Value, league.Name, league.CreatedAt);
    }
}
