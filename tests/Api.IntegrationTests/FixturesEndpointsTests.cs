using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class FixturesEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Fixtures");
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<Guid> CreateClubAsync(string name)
    {
        var resp = await _client.PostAsJsonAsync("/api/clubs", new { name });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    private async Task<Guid> CreateLeagueAsync(string name)
    {
        var resp = await _client.PostAsJsonAsync("/api/leagues", new { name });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    [Test]
    public async Task PostFixture_Returns201_AndGetReturns200WithExpectedValues()
    {
        var homeId = await CreateClubAsync("Fixture Home Club");
        var awayId = await CreateClubAsync("Fixture Away Club");
        var leagueId = await CreateLeagueAsync("Fixture League");

        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId = homeId,
            awayClubId = awayId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)1
        };

        var postResponse = await _client.PostAsJsonAsync("/api/fixtures", payload);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<FixtureResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.HomeClubId.Should().Be(homeId);
        created.AwayClubId.Should().Be(awayId);
        created.CompetitionId.Should().Be(leagueId);
        created.Status.Should().Be("Scheduled");

        var getResponse = await _client.GetAsync($"/api/fixtures/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<FixtureResponse>();
        fetched!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task GetFixtures_ByCompetition_ReturnsList()
    {
        var homeId = await CreateClubAsync("List Home Club");
        var awayId = await CreateClubAsync("List Away Club");
        var leagueId = await CreateLeagueAsync("Fixture List League");

        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId = homeId,
            awayClubId = awayId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)null
        };

        var postResponse = await _client.PostAsJsonAsync("/api/fixtures", payload);
        var created = await postResponse.Content.ReadFromJsonAsync<FixtureResponse>();

        var listResponse = await _client.GetAsync($"/api/fixtures?type=League&competitionId={leagueId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fixtures = await listResponse.Content.ReadFromJsonAsync<List<FixtureResponse>>();
        fixtures!.Should().Contain(f => f.Id == created!.Id);
    }

    [Test]
    public async Task RecordResult_Returns200_WithUpdatedScores()
    {
        var homeId = await CreateClubAsync("Result Home Club");
        var awayId = await CreateClubAsync("Result Away Club");
        var leagueId = await CreateLeagueAsync("Result League");

        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId = homeId,
            awayClubId = awayId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)null
        };

        var postResponse = await _client.PostAsJsonAsync("/api/fixtures", payload);
        var created = await postResponse.Content.ReadFromJsonAsync<FixtureResponse>();

        var resultResponse = await _client.PutAsJsonAsync(
            $"/api/fixtures/{created!.Id}/result",
            new { homeScore = 2, awayScore = 1 });

        resultResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await resultResponse.Content.ReadFromJsonAsync<FixtureResponse>();
        updated!.HomeScore.Should().Be(2);
        updated.AwayScore.Should().Be(1);
        updated.Status.Should().Be("Played");
    }

    [Test]
    public async Task RecordResult_NegativeScore_Returns400()
    {
        var homeId = await CreateClubAsync("BadScore Home Club");
        var awayId = await CreateClubAsync("BadScore Away Club");
        var leagueId = await CreateLeagueAsync("BadScore League");

        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId = homeId,
            awayClubId = awayId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)null
        };

        var postResponse = await _client.PostAsJsonAsync("/api/fixtures", payload);
        var created = await postResponse.Content.ReadFromJsonAsync<FixtureResponse>();

        var resultResponse = await _client.PutAsJsonAsync(
            $"/api/fixtures/{created!.Id}/result",
            new { homeScore = -1, awayScore = 0 });

        resultResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RescheduleFixture_Returns200_WithUpdatedSchedule()
    {
        var homeId = await CreateClubAsync("Reschedule Home Club");
        var awayId = await CreateClubAsync("Reschedule Away Club");
        var leagueId = await CreateLeagueAsync("Reschedule League");

        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId = homeId,
            awayClubId = awayId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)null
        };

        var postResponse = await _client.PostAsJsonAsync("/api/fixtures", payload);
        var created = await postResponse.Content.ReadFromJsonAsync<FixtureResponse>();

        var newDate = new DateTimeOffset(2025, 9, 1, 15, 0, 0, TimeSpan.Zero);
        var scheduleResponse = await _client.PutAsJsonAsync(
            $"/api/fixtures/{created!.Id}/schedule",
            new { scheduledAt = newDate, venue = "Wembley" });

        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await scheduleResponse.Content.ReadFromJsonAsync<FixtureResponse>();
        updated!.Venue.Should().Be("Wembley");
    }

    private sealed record IdResponse(Guid Id);
    private sealed record FixtureResponse(
        Guid Id,
        string CompetitionType,
        Guid CompetitionId,
        Guid HomeClubId,
        Guid AwayClubId,
        DateTimeOffset? ScheduledAt,
        string? Venue,
        string Status,
        int? RoundNumber,
        int? HomeScore,
        int? AwayScore,
        DateTimeOffset? PlayedAt);
}
