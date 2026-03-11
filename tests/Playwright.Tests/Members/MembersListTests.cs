using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Playwright.Tests.Members;

/// <summary>
/// Tests for the /members list page.
///
/// Each test spins up its own isolated server + SQLite database so tests
/// are completely independent regardless of execution order.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class MembersListTests : PageTest
{
    private PlaywrightServerFactory _factory = null!;
    private string _baseUrl = "";

    // Our [SetUp] runs after PageTest's [SetUp] (base runs first in NUnit),
    // so the Playwright Page is ready before we start the server.
    [SetUp]
    public void StartServer()
    {
        _factory = new PlaywrightServerFactory($"List_{Guid.NewGuid():N}");
        _baseUrl = _factory.BaseUrl;
    }

    [TearDown]
    public void StopServer() => _factory?.Dispose();

    // ── Rendering ─────────────────────────────────────────────────────────────

    [Test]
    public async Task EmptyList_ShowsNoMembersMessage()
    {
        await Page.GotoAsync($"{_baseUrl}/members");

        await Expect(Page.GetByText("No members yet")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Add the first one." }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task EmptyList_AddMemberLinkIsPresent()
    {
        await Page.GotoAsync($"{_baseUrl}/members");

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Add Member" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task WithMember_ShowsMemberInTable()
    {
        await _factory.CreateMemberAsync("Alice Smith", "alice@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Alice Smith" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "alice@example.com" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task WithMember_EditLinkNavigatesToEditPage()
    {
        var id = await _factory.CreateMemberAsync("Bob Jones", "bob@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Bob Jones" })).ToBeVisibleAsync();

        await Page.Locator($"a[href='/members/{id}/edit']").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex($"/members/{id}/edit"));
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Test]
    public async Task Pagination_PreviousButtonDisabledOnFirstPage()
    {
        await _factory.CreateMemberAsync("Carol", "carol@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");

        var prevButton = Page.GetByRole(AriaRole.Button, new() { Name = "Previous page" });
        await Expect(prevButton).ToBeVisibleAsync();
        await Expect(prevButton).ToBeDisabledAsync();
    }

    [Test]
    public async Task Pagination_NextButtonDisabledWhenFewerThanPageSize()
    {
        await _factory.CreateMemberAsync("Dave", "dave@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");

        var nextButton = Page.GetByRole(AriaRole.Button, new() { Name = "Next page" });
        await Expect(nextButton).ToBeDisabledAsync();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Test]
    public async Task ClickDelete_ShowsConfirmationDialog()
    {
        await _factory.CreateMemberAsync("Eve", "eve@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");
        var dialog = Page.GetByText("Are you sure you want to delete");
        await ClickUntilVisibleAsync(
            Page.Locator("button.mud-icon-button-color-error").First, dialog);

        await Expect(Page.GetByText("Are you sure you want to delete")).ToBeVisibleAsync();
        await Expect(Page.GetByText("This cannot be undone.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ClickCancelOnDialog_DismissesDialog()
    {
        await _factory.CreateMemberAsync("Frank", "frank@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");
        var dialog = Page.GetByText("Are you sure you want to delete");
        await ClickUntilVisibleAsync(
            Page.Locator("button.mud-icon-button-color-error").First, dialog);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Frank" }).First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ClickConfirmOnDialog_DeletesMember()
    {
        await _factory.CreateMemberAsync("Grace", "grace@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");
        var dialog = Page.GetByText("Are you sure you want to delete");
        await ClickUntilVisibleAsync(
            Page.Locator("button.mud-icon-button-color-error").First, dialog);

        // The Delete button is inside the dialog (ConfirmText = "Delete")
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Grace" }).First).Not.ToBeVisibleAsync();
        await Expect(dialog).Not.ToBeVisibleAsync();
    }

    // ── Security ──────────────────────────────────────────────────────────────

    [Test]
    public async Task MemberName_WithHtmlTags_IsHtmlEncoded()
    {
        // Blazor encodes dynamic content; this verifies no XSS execution.
        await _factory.CreateMemberAsync("<script>alert('xss')</script>", "xss@example.com");

        await Page.GotoAsync($"{_baseUrl}/members");

        // The text should appear as literal characters, not as a live <script> tag.
        await Expect(Page.GetByText("<script>alert('xss')</script>")).ToBeVisibleAsync();

        // If a dialog opened, that would indicate script execution.
        await Expect(Page.Locator("dialog")).Not.ToBeVisibleAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Retries clicking a button every 500 ms until the expected element becomes
    /// visible.  This handles the Blazor SSR→interactive transition where the
    /// button is rendered by SSR (visible) but the @onclick handler is only
    /// attached after the circuit connects.
    /// </summary>
    private async Task ClickUntilVisibleAsync(ILocator button, ILocator waitFor,
        int timeoutMs = 15_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            await button.ClickAsync(new() { Force = true, Timeout = 1000 });
            if (await waitFor.IsVisibleAsync(new() { Timeout = 500 }))
                return;
            await Page.WaitForTimeoutAsync(300);
        }
        // Final assertion — will produce a readable failure message.
        await Expect(waitFor).ToBeVisibleAsync();
    }
}
