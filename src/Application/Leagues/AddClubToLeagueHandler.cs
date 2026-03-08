using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Leagues;

namespace ClubMonitor.Application.Leagues;

public sealed record AddClubToLeagueCommand(Guid LeagueId, Guid ClubId);

public sealed record LeagueEntryDto(Guid Id, Guid LeagueId, Guid ClubId, DateTimeOffset EnteredAt);

public sealed class AddClubToLeagueHandler(ILeagueRepository leagueRepo, IClubRepository clubRepo)
{
    public async Task<LeagueEntryDto> HandleAsync(AddClubToLeagueCommand command, CancellationToken ct = default)
    {
        var leagueId = LeagueId.From(command.LeagueId);
        var clubId = ClubId.From(command.ClubId);

        var league = await leagueRepo.FindByIdAsync(leagueId, ct);
        if (league is null)
            throw new ArgumentException($"League '{command.LeagueId}' not found.", nameof(command.LeagueId));

        var club = await clubRepo.FindByIdAsync(clubId, ct);
        if (club is null)
            throw new ArgumentException($"Club '{command.ClubId}' not found.", nameof(command.ClubId));

        if (await leagueRepo.EntryExistsAsync(leagueId, clubId, ct))
            throw new DuplicateLeagueEntryException(command.LeagueId, command.ClubId);

        var entry = LeagueEntry.Create(leagueId, clubId);
        await leagueRepo.AddEntryAsync(entry, ct);
        await leagueRepo.SaveChangesAsync(ct);

        return new LeagueEntryDto(entry.Id.Value, entry.LeagueId.Value, entry.ClubId.Value, entry.EnteredAt);
    }
}
