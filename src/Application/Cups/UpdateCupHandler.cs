using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record UpdateCupCommand(Guid Id, string Name);

public sealed class UpdateCupHandler(ICupRepository repo)
{
    public async Task<CupDto?> HandleAsync(UpdateCupCommand command, CancellationToken ct = default)
    {
        var cup = await repo.FindByIdAsync(CupId.From(command.Id), ct);
        if (cup is null) return null;

        var newName = command.Name.Trim();
        if (!string.Equals(cup.Name, newName, StringComparison.OrdinalIgnoreCase))
        {
            if (await repo.ExistsWithNameAsync(newName, ct))
                throw new DuplicateCupNameException(newName);
        }

        cup.Rename(newName);
        await repo.SaveChangesAsync(ct);

        return new CupDto(cup.Id.Value, cup.Name, cup.Status, cup.CreatedAt);
    }
}
