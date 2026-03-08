using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Domain.Fixtures;

public sealed class Fixture
{
    public FixtureId Id { get; private set; }
    public CompetitionType CompetitionType { get; private set; }
    public Guid CompetitionId { get; private set; }
    public ClubId HomeClubId { get; private set; }
    public ClubId AwayClubId { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public string? Venue { get; private set; }
    public FixtureStatus Status { get; private set; }
    public int? RoundNumber { get; private set; }
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }
    public DateTimeOffset? PlayedAt { get; private set; }

    private Fixture() { }

    public static Fixture Create(
        CompetitionType competitionType,
        Guid competitionId,
        ClubId homeClubId,
        ClubId awayClubId,
        DateTimeOffset? scheduledAt = null,
        string? venue = null,
        int? roundNumber = null)
    {
        if (homeClubId == awayClubId)
            throw new ArgumentException("Home and away clubs cannot be the same.");

        return new Fixture
        {
            Id = FixtureId.New(),
            CompetitionType = competitionType,
            CompetitionId = competitionId,
            HomeClubId = homeClubId,
            AwayClubId = awayClubId,
            ScheduledAt = scheduledAt,
            Venue = venue,
            Status = FixtureStatus.Scheduled,
            RoundNumber = roundNumber
        };
    }

    public void Reschedule(DateTimeOffset scheduledAt, string? venue = null)
    {
        ScheduledAt = scheduledAt;
        if (venue is not null)
            Venue = venue;
        if (Status == FixtureStatus.Cancelled)
            Status = FixtureStatus.Scheduled;
    }

    public void RecordResult(int homeScore, int awayScore)
    {
        if (homeScore < 0)
            throw new ArgumentException("Home score cannot be negative.", nameof(homeScore));
        if (awayScore < 0)
            throw new ArgumentException("Away score cannot be negative.", nameof(awayScore));
        HomeScore = homeScore;
        AwayScore = awayScore;
        PlayedAt = DateTimeOffset.UtcNow;
        Status = FixtureStatus.Played;
    }
}
