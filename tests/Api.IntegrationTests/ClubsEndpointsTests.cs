using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class ClubsEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Clubs");
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

    [Test]
    public async Task PostClub_Returns201_AndGetReturns200WithExpectedValues()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/clubs", new { name = "Arsenal" });

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<ClubResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Arsenal");

        var getResponse = await _client.GetAsync($"/api/clubs/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var club = await getResponse.Content.ReadFromJsonAsync<ClubResponse>();
        club.Should().NotBeNull();
        club!.Id.Should().Be(created.Id);
        club.Name.Should().Be("Arsenal");
    }

    [Test]
    public async Task PostClub_DuplicateName_Returns409Conflict()
    {
        await _client.PostAsJsonAsync("/api/clubs", new { name = "Chelsea" });
        var second = await _client.PostAsJsonAsync("/api/clubs", new { name = "Chelsea" });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetClubs_ReturnsList_ContainingCreatedClub()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/clubs", new { name = "Liverpool" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postResponse.Content.ReadFromJsonAsync<ClubResponse>();

        var listResponse = await _client.GetAsync("/api/clubs?skip=0&take=50");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clubs = await listResponse.Content.ReadFromJsonAsync<List<ClubResponse>>();
        clubs.Should().NotBeNull();
        clubs!.Should().Contain(c => c.Id == created!.Id);
    }

    [Test]
    public async Task PutClub_UpdatesName_AndGetReturnsUpdatedValue()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/clubs", new { name = "ManUtd" });
        var created = await postResponse.Content.ReadFromJsonAsync<ClubResponse>();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/clubs/{created!.Id}", new { name = "Manchester United" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/clubs/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ClubResponse>();
        updated!.Name.Should().Be("Manchester United");
    }

    [Test]
    public async Task DeleteClub_Returns204_AndGetReturns404()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/clubs", new { name = "Fulham" });
        var created = await postResponse.Content.ReadFromJsonAsync<ClubResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/clubs/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/clubs/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PutClub_DuplicateName_Returns409Conflict()
    {
        await _client.PostAsJsonAsync("/api/clubs", new { name = "Everton" });

        var secondPost = await _client.PostAsJsonAsync("/api/clubs", new { name = "Burnley" });
        var second = await secondPost.Content.ReadFromJsonAsync<ClubResponse>();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/clubs/{second!.Id}", new { name = "Everton" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record ClubResponse(Guid Id, string Name, DateTimeOffset CreatedAt);
}
