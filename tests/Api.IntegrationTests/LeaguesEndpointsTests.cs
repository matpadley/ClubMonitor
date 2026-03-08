using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class LeaguesEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Leagues");
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

    [Test]
    public async Task PostLeague_Returns201_AndGetReturns200WithExpectedValues()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/leagues", new { name = "Premier League" });

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<LeagueResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Premier League");

        var getResponse = await _client.GetAsync($"/api/leagues/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var league = await getResponse.Content.ReadFromJsonAsync<LeagueResponse>();
        league!.Name.Should().Be("Premier League");
    }

    [Test]
    public async Task PostLeague_DuplicateName_Returns409Conflict()
    {
        await _client.PostAsJsonAsync("/api/leagues", new { name = "Championship" });
        var second = await _client.PostAsJsonAsync("/api/leagues", new { name = "Championship" });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetLeagues_ReturnsList_ContainingCreatedLeague()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/leagues", new { name = "League One" });
        var created = await postResponse.Content.ReadFromJsonAsync<LeagueResponse>();

        var listResponse = await _client.GetAsync("/api/leagues?skip=0&take=50");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var leagues = await listResponse.Content.ReadFromJsonAsync<List<LeagueResponse>>();
        leagues!.Should().Contain(l => l.Id == created!.Id);
    }

    [Test]
    public async Task PutLeague_UpdatesName()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/leagues", new { name = "League Two" });
        var created = await postResponse.Content.ReadFromJsonAsync<LeagueResponse>();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/leagues/{created!.Id}", new { name = "League Two Renamed" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/leagues/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<LeagueResponse>();
        updated!.Name.Should().Be("League Two Renamed");
    }

    [Test]
    public async Task DeleteLeague_Returns204_AndGetReturns404()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/leagues", new { name = "Delete League" });
        var created = await postResponse.Content.ReadFromJsonAsync<LeagueResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/leagues/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/leagues/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AddClubToLeague_Returns201_AndListContainsEntry()
    {
        var leaguePost = await _client.PostAsJsonAsync("/api/leagues", new { name = "Entry League" });
        var league = await leaguePost.Content.ReadFromJsonAsync<LeagueResponse>();
        var clubId = await CreateClubAsync("Entry Club");

        var addResponse = await _client.PostAsJsonAsync(
            $"/api/leagues/{league!.Id}/clubs", new { clubId });

        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _client.GetAsync($"/api/leagues/{league.Id}/clubs");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = await listResponse.Content.ReadFromJsonAsync<List<LeagueEntryResponse>>();
        entries!.Should().Contain(e => e.ClubId == clubId);
    }

    [Test]
    public async Task AddClubToLeague_Duplicate_Returns409Conflict()
    {
        var leaguePost = await _client.PostAsJsonAsync("/api/leagues", new { name = "Dup Entry League" });
        var league = await leaguePost.Content.ReadFromJsonAsync<LeagueResponse>();
        var clubId = await CreateClubAsync("Dup Club League");

        await _client.PostAsJsonAsync($"/api/leagues/{league!.Id}/clubs", new { clubId });
        var second = await _client.PostAsJsonAsync($"/api/leagues/{league.Id}/clubs", new { clubId });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RemoveClubFromLeague_Returns204()
    {
        var leaguePost = await _client.PostAsJsonAsync("/api/leagues", new { name = "Remove Entry League" });
        var league = await leaguePost.Content.ReadFromJsonAsync<LeagueResponse>();
        var clubId = await CreateClubAsync("Remove Club League");

        await _client.PostAsJsonAsync($"/api/leagues/{league!.Id}/clubs", new { clubId });

        var deleteResponse = await _client.DeleteAsync($"/api/leagues/{league.Id}/clubs/{clubId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync($"/api/leagues/{league.Id}/clubs");
        var entries = await listResponse.Content.ReadFromJsonAsync<List<LeagueEntryResponse>>();
        entries!.Should().NotContain(e => e.ClubId == clubId);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record LeagueResponse(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record LeagueEntryResponse(Guid Id, Guid LeagueId, Guid ClubId, DateTimeOffset EnteredAt);
}
