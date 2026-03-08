using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record AddClubToCupCommand(Guid CupId, Guid ClubId);

public sealed record CupEntryDto(Guid Id, Guid CupId, Guid ClubId, DateTimeOffset EnteredAt);

public sealed class AddClubToCupHandler(ICupRepository cupRepo, IClubRepository clubRepo)
{
    public async Task<CupEntryDto> HandleAsync(AddClubToCupCommand command, CancellationToken ct = default)
    {
        var cupId = CupId.From(command.CupId);
        var clubId = ClubId.From(command.ClubId);

        var cup = await cupRepo.FindByIdAsync(cupId, ct);
        if (cup is null)
            throw new ArgumentException($"Cup '{command.CupId}' not found.", nameof(command.CupId));

        if (cup.Status != CupStatus.Draft)
            throw new InvalidCupStateException($"Cannot add clubs to a cup that is not in Draft status.");

        var club = await clubRepo.FindByIdAsync(clubId, ct);
        if (club is null)
            throw new ArgumentException($"Club '{command.ClubId}' not found.", nameof(command.ClubId));

        if (await cupRepo.EntryExistsAsync(cupId, clubId, ct))
            throw new DuplicateCupEntryException(command.CupId, command.ClubId);

        var entry = CupEntry.Create(cupId, clubId);
        await cupRepo.AddEntryAsync(entry, ct);
        await cupRepo.SaveChangesAsync(ct);

        return new CupEntryDto(entry.Id.Value, entry.CupId.Value, entry.ClubId.Value, entry.EnteredAt);
    }
}
