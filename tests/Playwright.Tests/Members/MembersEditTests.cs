using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Playwright.Tests.Members;

/// <summary>
/// Tests for the /members/{id}/edit page.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class MembersEditTests : PageTest
{
    private PlaywrightServerFactory _factory = null!;
    private string _baseUrl = "";

    [SetUp]
    public void StartServer()
    {
        _factory = new PlaywrightServerFactory($"Edit_{Guid.NewGuid():N}");
        _baseUrl = _factory.BaseUrl;
    }

    [TearDown]
    public void StopServer() => _factory.Dispose();

    // ── Not-found ─────────────────────────────────────────────────────────────

    [Test]
    public async Task EditWithUnknownId_ShowsNotFoundMessage()
    {
        var unknownId = Guid.NewGuid();

        await Page.GotoAsync($"{_baseUrl}/members/{unknownId}/edit");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Member Not Found" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("The requested member does not exist.")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Back to Members" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task NotFoundPage_BackLinkNavigatesToList()
    {
        var unknownId = Guid.NewGuid();

        await Page.GotoAsync($"{_baseUrl}/members/{unknownId}/edit");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Back to Members" }))
            .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Back to Members" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    [Test]
    public async Task EditWithValidId_PrePopulatesFormFields()
    {
        var id = await _factory.CreateMemberAsync("Jane Doe", "jane@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");

        var nameField = Page.GetByLabel("Name");
        var emailField = Page.GetByLabel("Email");

        await Expect(nameField).ToBeVisibleAsync();
        await Expect(nameField).ToHaveValueAsync("Jane Doe");
        await Expect(emailField).ToHaveValueAsync("jane@example.com");
    }

    [Test]
    public async Task EditPage_HasSaveAndCancelControls()
    {
        var id = await _factory.CreateMemberAsync("Ken", "ken@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Cancel" })).ToBeVisibleAsync();
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateName_SavesAndRedirectsToList()
    {
        var id = await _factory.CreateMemberAsync("Old Name", "update@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").ClearAsync();
        await Page.GetByLabel("Name").FillAsync("New Name");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "New Name" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task UpdateEmail_SavesAndRedirectsToList()
    {
        var id = await _factory.CreateMemberAsync("Laura", "old@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();

        await Page.GetByLabel("Email").ClearAsync();
        await Page.GetByLabel("Email").FillAsync("new@example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "new@example.com" }))
            .ToBeVisibleAsync();
    }

    // ── Error handling ────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateWithDuplicateEmail_ShowsErrorMessage()
    {
        await _factory.CreateMemberAsync("Existing", "taken@example.com");
        var id = await _factory.CreateMemberAsync("Mike", "mike@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();

        await Page.GetByLabel("Email").ClearAsync();
        await Page.GetByLabel("Email").FillAsync("taken@example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page.GetByText("already exists")).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync(new Regex($"/members/{id}/edit$"));
    }

    [Test]
    public async Task UpdateWithEmptyName_ShowsValidationError()
    {
        var id = await _factory.CreateMemberAsync("Nancy", "nancy@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").ClearAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page.Locator(".validation-message").First).ToBeVisibleAsync();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    [Test]
    public async Task CancelButton_NavigatesToMembersList()
    {
        var id = await _factory.CreateMemberAsync("Oscar", "oscar@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Cancel" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
    }

    // ── Security ──────────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateNameWithHtmlTags_IsStoredAndDisplayedSafely()
    {
        var id = await _factory.CreateMemberAsync("Plain Name", "safe@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/{id}/edit");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").ClearAsync();
        await Page.GetByLabel("Name").FillAsync("<img src=x onerror=alert(1)>");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Blazor HTML-encodes dynamic values; the literal string must appear.
        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
        await Expect(Page.GetByText("<img src=x onerror=alert(1)>")).ToBeVisibleAsync();
    }
}
