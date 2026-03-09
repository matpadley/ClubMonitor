using ClubMonitor.Domain.Cups;
using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Cups;

public sealed record DrawCupCommand(Guid CupId);

public sealed record DrawCupResult(CupDto Cup, IReadOnlyList<FixtureDto> Fixtures);

public sealed record FixtureDto(
    Guid Id,
    CompetitionType CompetitionType,
    Guid CompetitionId,
    Guid HomeClubId,
    Guid AwayClubId,
    DateTimeOffset? ScheduledAt,
    string? Venue,
    FixtureStatus Status,
    int? RoundNumber,
    int? HomeScore,
    int? AwayScore,
    DateTimeOffset? PlayedAt);

public sealed class DrawCupHandler(ICupRepository cupRepo, IFixtureRepository fixtureRepo)
{
    public async Task<DrawCupResult> HandleAsync(DrawCupCommand command, CancellationToken ct = default)
    {
        var cupId = CupId.From(command.CupId);
        var cup = await cupRepo.FindByIdAsync(cupId, ct);
        if (cup is null)
            throw new ArgumentException($"Cup '{command.CupId}' not found.", nameof(command.CupId));

        if (cup.Status != CupStatus.Draft)
            throw new InvalidCupStateException($"Cup '{cup.Name}' has already been drawn.");

        var entries = await cupRepo.ListEntriesAsync(cupId, 0, int.MaxValue, ct);
        if (entries.Count < 2)
            throw new InvalidCupStateException("A cup draw requires at least 2 entered clubs.");

        // Randomly shuffle entries and create round-1 fixture pairings
        var shuffled = entries.OrderBy(_ => Random.Shared.Next()).ToList();
        var fixtures = new List<Fixture>();

        for (var i = 0; i + 1 < shuffled.Count; i += 2)
        {
            var fixture = Fixture.Create(
                CompetitionType.Cup,
                cup.Id.Value,
                shuffled[i].ClubId,
                shuffled[i + 1].ClubId,
                scheduledAt: null,
                venue: null,
                roundNumber: 1);
            fixtures.Add(fixture);
        }

        await fixtureRepo.AddRangeAsync(fixtures, ct);
        cup.MarkDrawn();
        await cupRepo.SaveChangesAsync(ct);

        var cupDto = new CupDto(cup.Id.Value, cup.Name, cup.Status, cup.CreatedAt);
        var fixtureDtos = fixtures.Select(f => new FixtureDto(
            f.Id.Value, f.CompetitionType, f.CompetitionId,
            f.HomeClubId.Value, f.AwayClubId.Value,
            f.ScheduledAt, f.Venue, f.Status, f.RoundNumber,
            f.HomeScore, f.AwayScore, f.PlayedAt)).ToList();

        return new DrawCupResult(cupDto, fixtureDtos);
    }
}
