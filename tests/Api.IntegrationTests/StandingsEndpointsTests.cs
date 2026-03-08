using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class StandingsEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Standings");
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
        return (await resp.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    private async Task<Guid> CreateLeagueAsync(string name)
    {
        var resp = await _client.PostAsJsonAsync("/api/leagues", new { name });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    private async Task AddClubToLeagueAsync(Guid leagueId, Guid clubId)
    {
        var resp = await _client.PostAsJsonAsync($"/api/leagues/{leagueId}/clubs", new { clubId });
        resp.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateFixtureAsync(Guid leagueId, Guid homeClubId, Guid awayClubId)
    {
        var payload = new
        {
            competitionType = "League",
            competitionId = leagueId,
            homeClubId,
            awayClubId,
            scheduledAt = (DateTimeOffset?)null,
            venue = (string?)null,
            roundNumber = (int?)1
        };
        var resp = await _client.PostAsJsonAsync("/api/fixtures", payload);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    [Test]
    public async Task GetStandings_AfterRecordedResult_ReflectsCorrectPoints()
    {
        var leagueId = await CreateLeagueAsync("Standings League");
        var homeId = await CreateClubAsync("Standings Home FC");
        var awayId = await CreateClubAsync("Standings Away FC");

        await AddClubToLeagueAsync(leagueId, homeId);
        await AddClubToLeagueAsync(leagueId, awayId);

        var fixtureId = await CreateFixtureAsync(leagueId, homeId, awayId);

        // Home wins 2-0
        var resultResponse = await _client.PutAsJsonAsync(
            $"/api/fixtures/{fixtureId}/result",
            new { homeScore = 2, awayScore = 0 });
        resultResponse.EnsureSuccessStatusCode();

        var standingsResponse = await _client.GetAsync($"/api/leagues/{leagueId}/standings");
        standingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var standings = await standingsResponse.Content.ReadFromJsonAsync<List<StandingResponse>>();
        standings.Should().NotBeNull();
        standings!.Should().HaveCount(2);

        var homeStanding = standings!.Single(s => s.ClubId == homeId);
        homeStanding.Played.Should().Be(1);
        homeStanding.Won.Should().Be(1);
        homeStanding.Points.Should().Be(3);
        homeStanding.GoalsFor.Should().Be(2);
        homeStanding.GoalsAgainst.Should().Be(0);

        var awayStanding = standings!.Single(s => s.ClubId == awayId);
        awayStanding.Played.Should().Be(1);
        awayStanding.Lost.Should().Be(1);
        awayStanding.Points.Should().Be(0);
    }

    [Test]
    public async Task GetStandings_DrawResult_EachClubGetsOnePoint()
    {
        var leagueId = await CreateLeagueAsync("Draw Standings League");
        var homeId = await CreateClubAsync("Draw Home FC");
        var awayId = await CreateClubAsync("Draw Away FC");

        await AddClubToLeagueAsync(leagueId, homeId);
        await AddClubToLeagueAsync(leagueId, awayId);

        var fixtureId = await CreateFixtureAsync(leagueId, homeId, awayId);

        await _client.PutAsJsonAsync(
            $"/api/fixtures/{fixtureId}/result",
            new { homeScore = 1, awayScore = 1 });

        var standingsResponse = await _client.GetAsync($"/api/leagues/{leagueId}/standings");
        var standings = await standingsResponse.Content.ReadFromJsonAsync<List<StandingResponse>>();

        standings!.Should().AllSatisfy(s =>
        {
            s.Points.Should().Be(1);
            s.Drawn.Should().Be(1);
        });
    }

    [Test]
    public async Task GetStandings_NoFixtures_AllZeroStats()
    {
        var leagueId = await CreateLeagueAsync("Empty Standings League");
        var club1Id = await CreateClubAsync("Empty Standings Club A");
        var club2Id = await CreateClubAsync("Empty Standings Club B");

        await AddClubToLeagueAsync(leagueId, club1Id);
        await AddClubToLeagueAsync(leagueId, club2Id);

        var standingsResponse = await _client.GetAsync($"/api/leagues/{leagueId}/standings");
        standingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var standings = await standingsResponse.Content.ReadFromJsonAsync<List<StandingResponse>>();
        standings!.Should().HaveCount(2);
        standings.Should().AllSatisfy(s =>
        {
            s.Played.Should().Be(0);
            s.Points.Should().Be(0);
        });
    }

    private sealed record IdResponse(Guid Id);
    private sealed record StandingResponse(
        Guid ClubId,
        string ClubName,
        int Played,
        int Won,
        int Drawn,
        int Lost,
        int GoalsFor,
        int GoalsAgainst,
        int GoalDifference,
        int Points);
}
