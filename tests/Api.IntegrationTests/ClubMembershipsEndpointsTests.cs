using System.Net;
using System.Net.Http.Json;
using ClubMonitor.Domain.Clubs;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Api.IntegrationTests;

[TestFixture]
public sealed class ClubMembershipsEndpointsTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new TestWebApplicationFactory("ClubMonitorTest_Memberships");
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

    private async Task<Guid> CreateMemberAsync(string name, string email)
    {
        var resp = await _client.PostAsJsonAsync("/api/members", new { name, email });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    private async Task<Guid> CreateClubAsync(string name)
    {
        var resp = await _client.PostAsJsonAsync("/api/clubs", new { name });
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    [Test]
    public async Task AddMemberToClub_Returns201_AndListContainsMember()
    {
        var memberId = await CreateMemberAsync("John Doe", "johndoe@example.com");
        var clubId = await CreateClubAsync("Northside FC");

        var postResponse = await _client.PostAsJsonAsync(
            $"/api/clubs/{clubId}/members",
            new { memberId, role = "Player" });

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var membership = await postResponse.Content.ReadFromJsonAsync<MembershipResponse>();
        membership.Should().NotBeNull();
        membership!.MemberId.Should().Be(memberId);
        membership.ClubId.Should().Be(clubId);

        var listResponse = await _client.GetAsync($"/api/clubs/{clubId}/members");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await listResponse.Content.ReadFromJsonAsync<List<MembershipResponse>>();
        members!.Should().Contain(m => m.MemberId == memberId);
    }

    [Test]
    public async Task AddMemberToClub_DuplicateMembership_Returns409Conflict()
    {
        var memberId = await CreateMemberAsync("Dup Member", "dupmember@example.com");
        var clubId = await CreateClubAsync("Southside FC");

        await _client.PostAsJsonAsync($"/api/clubs/{clubId}/members", new { memberId, role = "Player" });
        var second = await _client.PostAsJsonAsync($"/api/clubs/{clubId}/members", new { memberId, role = "Captain" });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RemoveMemberFromClub_Returns204_AndListNoLongerContainsMember()
    {
        var memberId = await CreateMemberAsync("Remove Me", "removeme@example.com");
        var clubId = await CreateClubAsync("Eastside FC");

        await _client.PostAsJsonAsync($"/api/clubs/{clubId}/members", new { memberId, role = "Player" });

        var deleteResponse = await _client.DeleteAsync($"/api/clubs/{clubId}/members/{memberId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync($"/api/clubs/{clubId}/members");
        var members = await listResponse.Content.ReadFromJsonAsync<List<MembershipResponse>>();
        members!.Should().NotContain(m => m.MemberId == memberId);
    }

    [Test]
    public async Task RemoveMemberNotInClub_Returns404()
    {
        var clubId = await CreateClubAsync("Westside FC");
        var unknownMemberId = Guid.NewGuid();

        var deleteResponse = await _client.DeleteAsync($"/api/clubs/{clubId}/members/{unknownMemberId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record IdResponse(Guid Id);
    private sealed record MembershipResponse(Guid Id, Guid ClubId, Guid MemberId, string Role, DateTimeOffset JoinedAt);
}
