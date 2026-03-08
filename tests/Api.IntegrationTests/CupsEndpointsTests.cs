using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class CupsEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Cups");
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

    private async Task<CupResponse> CreateCupAsync(string name)
    {
        var resp = await _client.PostAsJsonAsync("/api/cups", new { name });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<CupResponse>())!;
    }

    [Test]
    public async Task PostCup_Returns201_AndGetReturns200WithExpectedValues()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/cups", new { name = "FA Cup" });

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<CupResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("FA Cup");
        created.Status.Should().Be("Draft");

        var getResponse = await _client.GetAsync($"/api/cups/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cup = await getResponse.Content.ReadFromJsonAsync<CupResponse>();
        cup!.Name.Should().Be("FA Cup");
    }

    [Test]
    public async Task PostCup_DuplicateName_Returns409Conflict()
    {
        await _client.PostAsJsonAsync("/api/cups", new { name = "League Cup" });
        var second = await _client.PostAsJsonAsync("/api/cups", new { name = "League Cup" });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetCups_ReturnsList_ContainingCreatedCup()
    {
        var cup = await CreateCupAsync("Carabao Cup");

        var listResponse = await _client.GetAsync("/api/cups?skip=0&take=50");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cups = await listResponse.Content.ReadFromJsonAsync<List<CupResponse>>();
        cups!.Should().Contain(c => c.Id == cup.Id);
    }

    [Test]
    public async Task PutCup_UpdatesName()
    {
        var cup = await CreateCupAsync("Old Cup Name");

        var putResponse = await _client.PutAsJsonAsync($"/api/cups/{cup.Id}", new { name = "New Cup Name" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/cups/{cup.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<CupResponse>();
        updated!.Name.Should().Be("New Cup Name");
    }

    [Test]
    public async Task DeleteCup_InDraftState_Returns204()
    {
        var cup = await CreateCupAsync("Delete Cup");

        var deleteResponse = await _client.DeleteAsync($"/api/cups/{cup.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/cups/{cup.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AddClubToCup_Returns201_AndListContainsEntry()
    {
        var cup = await CreateCupAsync("Entry Cup");
        var clubId = await CreateClubAsync("Entry Cup Club");

        var addResponse = await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId });
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _client.GetAsync($"/api/cups/{cup.Id}/clubs");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = await listResponse.Content.ReadFromJsonAsync<List<CupEntryResponse>>();
        entries!.Should().Contain(e => e.ClubId == clubId);
    }

    [Test]
    public async Task AddClubToCup_Duplicate_Returns409Conflict()
    {
        var cup = await CreateCupAsync("Dup Cup");
        var clubId = await CreateClubAsync("Dup Cup Club");

        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId });
        var second = await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RemoveClubFromCup_Returns204()
    {
        var cup = await CreateCupAsync("Remove Entry Cup");
        var clubId = await CreateClubAsync("Remove Entry Cup Club");

        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId });

        var deleteResponse = await _client.DeleteAsync($"/api/cups/{cup.Id}/clubs/{clubId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync($"/api/cups/{cup.Id}/clubs");
        var entries = await listResponse.Content.ReadFromJsonAsync<List<CupEntryResponse>>();
        entries!.Should().NotContain(e => e.ClubId == clubId);
    }

    [Test]
    public async Task DrawCup_WithTwoOrMoreClubs_Returns200WithFixtures()
    {
        var cup = await CreateCupAsync("Draw Cup");
        var club1Id = await CreateClubAsync("Draw Cup Club A");
        var club2Id = await CreateClubAsync("Draw Cup Club B");

        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId = club1Id });
        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId = club2Id });

        var drawResponse = await _client.PostAsync($"/api/cups/{cup.Id}/draw", null);
        drawResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await drawResponse.Content.ReadFromJsonAsync<DrawResult>();
        result.Should().NotBeNull();
        result!.Cup.Status.Should().Be("Drawn");
        result.Fixtures.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task DrawCup_DrawnCup_CannotAddMoreClubs()
    {
        var cup = await CreateCupAsync("Locked Cup");
        var club1Id = await CreateClubAsync("Locked Cup Club A");
        var club2Id = await CreateClubAsync("Locked Cup Club B");
        var club3Id = await CreateClubAsync("Locked Cup Club C");

        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId = club1Id });
        await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId = club2Id });
        await _client.PostAsync($"/api/cups/{cup.Id}/draw", null);

        var addAfterDraw = await _client.PostAsJsonAsync($"/api/cups/{cup.Id}/clubs", new { clubId = club3Id });
        addAfterDraw.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record CupResponse(Guid Id, string Name, string Status, DateTimeOffset CreatedAt);
    private sealed record CupEntryResponse(Guid Id, Guid CupId, Guid ClubId, DateTimeOffset EnteredAt);
    private sealed record FixtureResponse(Guid Id, Guid HomeClubId, Guid AwayClubId);
    private sealed record DrawResult(CupResponse Cup, List<FixtureResponse> Fixtures);
}
