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

    private sealed record MemberResponse(Guid Id, string Name, string Email, DateTimeOffset CreatedAt);
}
