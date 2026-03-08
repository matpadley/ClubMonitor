using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record DeleteCupCommand(Guid Id);

public sealed class DeleteCupHandler(ICupRepository repo)
{
    public async Task<bool> HandleAsync(DeleteCupCommand command, CancellationToken ct = default)
    {
        var cup = await repo.FindByIdAsync(CupId.From(command.Id), ct);
        if (cup is null) return false;

        if (cup.Status != CupStatus.Draft)
            throw new InvalidCupStateException("Only Draft cups can be deleted.");

        return await repo.DeleteAsync(CupId.From(command.Id), ct);
    }
}
