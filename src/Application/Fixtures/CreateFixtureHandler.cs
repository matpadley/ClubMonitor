using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Fixtures;

public sealed record CreateFixtureCommand(
    CompetitionType CompetitionType,
    Guid CompetitionId,
    Guid HomeClubId,
    Guid AwayClubId,
    DateTimeOffset? ScheduledAt,
    string? Venue,
    int? RoundNumber);

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

public sealed class CreateFixtureHandler(IFixtureRepository repo)
{
    public async Task<FixtureDto> HandleAsync(CreateFixtureCommand command, CancellationToken ct = default)
    {
        var fixture = Fixture.Create(
            command.CompetitionType,
            command.CompetitionId,
            ClubId.From(command.HomeClubId),
            ClubId.From(command.AwayClubId),
            command.ScheduledAt,
            command.Venue,
            command.RoundNumber);

        await repo.AddAsync(fixture, ct);
        await repo.SaveChangesAsync(ct);

        return ToDto(fixture);
    }

    internal static FixtureDto ToDto(Fixture f) => new(
        f.Id.Value, f.CompetitionType, f.CompetitionId,
        f.HomeClubId.Value, f.AwayClubId.Value,
        f.ScheduledAt, f.Venue, f.Status, f.RoundNumber,
        f.HomeScore, f.AwayScore, f.PlayedAt);
}
