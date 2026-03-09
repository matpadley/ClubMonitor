using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class MembersEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory();
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
    public async Task PostMember_Returns201_AndGetReturns200WithExpectedValues()
    {
        // POST /api/members
        var postResponse = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "Alice", email = "alice@example.com" });

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<MemberResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Alice");
        created.Email.Should().Be("alice@example.com");
        created.CreatedAt.Should().NotBe(default);

        // GET /api/members/{id}
        var getResponse = await _client.GetAsync($"/api/members/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var member = await getResponse.Content.ReadFromJsonAsync<MemberResponse>();
        member.Should().NotBeNull();
        member!.Id.Should().Be(created.Id);
        member.Name.Should().Be("Alice");
        member.Email.Should().Be("alice@example.com");
    }

    [Test]
    public async Task PostMember_DuplicateEmail_Returns409Conflict()
    {
        var payload = new { name = "Bob", email = "bob@example.com" };

        var first = await _client.PostAsJsonAsync("/api/members", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/members", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetMembers_ReturnsList_ContainingCreatedMember()
    {
        var postResponse = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "ListUser", email = "listuser@example.com" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postResponse.Content.ReadFromJsonAsync<MemberResponse>();

        var listResponse = await _client.GetAsync("/api/members?skip=0&take=50");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await listResponse.Content.ReadFromJsonAsync<List<MemberResponse>>();
        members.Should().NotBeNull();
        members!.Should().Contain(m => m.Id == created!.Id);
    }

    [Test]
    public async Task PutMember_UpdatesNameAndEmail_AndGetReturnsUpdatedValues()
    {
        var postResponse = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "UpdateUser", email = "updateuser@example.com" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postResponse.Content.ReadFromJsonAsync<MemberResponse>();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/members/{created!.Id}",
            new { name = "UpdatedUser", email = "updateduser@example.com" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/members/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var member = await getResponse.Content.ReadFromJsonAsync<MemberResponse>();
        member!.Name.Should().Be("UpdatedUser");
        member.Email.Should().Be("updateduser@example.com");
    }

    [Test]
    public async Task DeleteMember_Returns204_AndGetReturns404()
    {
        var postResponse = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "DeleteUser", email = "deleteuser@example.com" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postResponse.Content.ReadFromJsonAsync<MemberResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/members/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/members/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PutMember_DuplicateEmail_Returns409Conflict()
    {
        var firstPost = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "DupA", email = "dupa@example.com" });
        firstPost.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondPost = await _client.PostAsJsonAsync(
            "/api/members",
            new { name = "DupB", email = "dupb@example.com" });
        secondPost.StatusCode.Should().Be(HttpStatusCode.Created);
        var secondCreated = await secondPost.Content.ReadFromJsonAsync<MemberResponse>();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/members/{secondCreated!.Id}",
            new { name = "DupB", email = "dupa@example.com" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record MemberResponse(Guid Id, string Name, string Email, DateTimeOffset CreatedAt);
}
