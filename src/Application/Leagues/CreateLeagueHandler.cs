using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record CreateLeagueCommand(string Name);

public sealed record LeagueDto(Guid Id, string Name, DateTimeOffset CreatedAt);

public sealed class CreateLeagueHandler(ILeagueRepository repo)
{
    public async Task<LeagueDto> HandleAsync(CreateLeagueCommand command, CancellationToken ct = default)
    {
        if (await repo.ExistsWithNameAsync(command.Name, ct))
            throw new DuplicateLeagueNameException(command.Name);

        var league = League.Create(command.Name);
        await repo.AddAsync(league, ct);
        await repo.SaveChangesAsync(ct);

        return new LeagueDto(league.Id.Value, league.Name, league.CreatedAt);
    }
}
