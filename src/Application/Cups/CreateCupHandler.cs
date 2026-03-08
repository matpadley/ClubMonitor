using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record CreateCupCommand(string Name);

public sealed record CupDto(Guid Id, string Name, CupStatus Status, DateTimeOffset CreatedAt);

public sealed class CreateCupHandler(ICupRepository repo)
{
    public async Task<CupDto> HandleAsync(CreateCupCommand command, CancellationToken ct = default)
    {
        if (await repo.ExistsWithNameAsync(command.Name, ct))
            throw new DuplicateCupNameException(command.Name);

        var cup = Cup.Create(command.Name);
        await repo.AddAsync(cup, ct);
        await repo.SaveChangesAsync(ct);

        return new CupDto(cup.Id.Value, cup.Name, cup.Status, cup.CreatedAt);
    }
}
