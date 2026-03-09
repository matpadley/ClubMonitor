using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Playwright.Tests.Members;

/// <summary>
/// Tests for the /members/create page.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class MembersCreateTests : PageTest
{
    private PlaywrightServerFactory _factory = null!;
    private string _baseUrl = "";

    [SetUp]
    public void StartServer()
    {
        _factory = new PlaywrightServerFactory($"Create_{Guid.NewGuid():N}");
        _baseUrl = _factory.BaseUrl;
    }

    [TearDown]
    public void StopServer() => _factory.Dispose();

    // ── Rendering ─────────────────────────────────────────────────────────────

    [Test]
    public async Task CreatePage_ShowsEmptyForm()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");

        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Cancel" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreatePage_HasCorrectTitle()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");

        await Expect(Page).ToHaveTitleAsync("Add Member");
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Test]
    public async Task SubmitEmptyForm_ShowsValidationErrors()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // DataAnnotations validation messages should appear
        await Expect(Page.Locator(".validation-message").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task SubmitInvalidEmail_ShowsValidationError()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("Test User");
        await Page.GetByLabel("Email").FillAsync("not-an-email");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page.Locator(".validation-message").First).ToBeVisibleAsync();
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Test]
    public async Task SubmitValidData_CreatesAndRedirectsToList()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("Hannah Brown");
        await Page.GetByLabel("Email").FillAsync("hannah@example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Hannah Brown" }))
            .ToBeVisibleAsync();
    }

    // ── Error handling ────────────────────────────────────────────────────────

    [Test]
    public async Task SubmitDuplicateEmail_ShowsErrorMessage()
    {
        await _factory.CreateMemberAsync("Ivan", "ivan@example.com");

        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("Ivan Duplicate");
        await Page.GetByLabel("Email").FillAsync("ivan@example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        await Expect(Page.GetByText("already exists")).ToBeVisibleAsync();
        // Should stay on the create page
        await Expect(Page).ToHaveURLAsync(new Regex("/members/create$"));
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    [Test]
    public async Task CancelButton_NavigatesToMembersList()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Cancel" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
    }

    // ── Security ──────────────────────────────────────────────────────────────

    [Test]
    public async Task SubmitHtmlInName_IsStoredAndDisplayedSafely()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("<b>Bold</b>");
        await Page.GetByLabel("Email").FillAsync("bold@example.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Should end up on the list page showing the literal string, not rendered HTML.
        await Expect(Page).ToHaveURLAsync(new Regex("/members$"));
        await Expect(Page.GetByText("<b>Bold</b>")).ToBeVisibleAsync();
    }

    [Test]
    public async Task SubmitScriptInEmail_IsRejectedByValidation()
    {
        await Page.GotoAsync($"{_baseUrl}/members/create");
        await Expect(Page.GetByLabel("Name")).ToBeVisibleAsync();

        await Page.GetByLabel("Name").FillAsync("Attacker");
        await Page.GetByLabel("Email").FillAsync("<script>alert(1)</script>");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Invalid email format should be caught by DataAnnotations before hitting the server.
        await Expect(Page.Locator(".validation-message").First).ToBeVisibleAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/members/create$"));
    }
}
