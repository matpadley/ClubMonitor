using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record RemoveClubFromCupCommand(Guid CupId, Guid ClubId);

public sealed class RemoveClubFromCupHandler(ICupRepository cupRepo)
{
    public async Task<bool> HandleAsync(RemoveClubFromCupCommand command, CancellationToken ct = default)
    {
        var cup = await cupRepo.FindByIdAsync(CupId.From(command.CupId), ct);
        if (cup is null) return false;

        if (cup.Status != CupStatus.Draft)
            throw new InvalidCupStateException("Cannot remove clubs from a cup that is not in Draft status.");

        return await cupRepo.RemoveEntryAsync(CupId.From(command.CupId), ClubId.From(command.ClubId), ct);
    }
}
