# Members UI — Specification

**Date:** 2026-03-09
**Technology:** Blazor SSR with Interactive Server render mode (.NET 10) — MudBlazor component library
**Status:** Implemented

---

## 1. Overview

This document specifies the Blazor user interface for the Members feature of ClubMonitor.
It covers all pages, shared components, data flow, navigation, error handling, and file
locations. The implementation targets the existing `src/Client` project using Interactive
Server render mode, injecting Application layer handlers directly (no HTTP client required).

The Members UI provides full CRUD capability:

| Operation | Description |
|-----------|-------------|
| List      | Paginated table of all members |
| Create    | Form to add a new member |
| Edit      | Pre-populated form to update an existing member |
| Delete    | Inline confirmation dialog before permanent removal |

---

## 2. Architecture Context

### 2.1 Blazor Hosting Model

The `src/Api` project hosts Blazor SSR via `MapRazorComponents<Api.Components.App>()` with
`.AddInteractiveServerRenderMode()`. Pages that require interactivity (all three Members pages)
declare `@rendermode InteractiveServer`, which establishes a persistent SignalR circuit between
browser and server.

### 2.2 Handler Injection

Because Blazor Server runs in-process with the Api host, Blazor components can inject
Application layer handlers directly from the DI container. No HTTP calls or API client are
needed for the Members UI.

| Handler Injected | Where Used |
|-----------------|------------|
| `ListMembersHandler` | `MemberList.razor` |
| `DeleteMemberHandler` | `MemberList.razor` |
| `CreateMemberHandler` | `MemberCreate.razor` |
| `GetMemberByIdHandler` | `MemberEdit.razor` |
| `UpdateMemberHandler` | `MemberEdit.razor` |

### 2.3 Scoped Services in Blazor Server

All handlers are registered as `Scoped`. In Blazor Server, a scope maps to a SignalR circuit
(one per user session). Handlers are stateless so there are no isolation concerns.

---

## 3. File Structure

```
src/Client/Components/
├── App.razor                                    (updated — Members UI integration)
├── _Imports.razor                               (updated — new usings added)
├── Layout/
│   └── MainLayout.razor                         (updated — MudBlazor drawer navigation)
├── Shared/
│   └── ConfirmDialog.razor                      (MudBlazor dialog used for delete confirmation)
└── Pages/
    └── Members/
        ├── MemberList.razor                     @page "/members"
        ├── MemberCreate.razor                   @page "/members/create"
        ├── MemberEdit.razor                     @page "/members/{Id:guid}/edit"
        └── Shared/
            ├── MemberFormModel.cs               (form model with DataAnnotations)
            ├── MemberForm.razor                 (shared form fields component)
            └── DeleteConfirmDialog.razor        (unused — kept as stub only)

docs/
└── members-ui-specification.md                  (this document)
```

---

## 4. Navigation — MainLayout.razor

`MainLayout.razor` uses MudBlazor's layout system with a collapsible `MudDrawer` and a
`MudAppBar` containing a hamburger toggle. The drawer lists implemented navigation sections.

```
┌─────────────────────────────────────────────────────────┐
│ ☰  ClubMonitor                                          │  ← MudAppBar
├───────────────┬─────────────────────────────────────────┤
│  👥 Members   │  <page content>                         │
│  👨‍👦 Clubs     │                                         │
└───────────────┴─────────────────────────────────────────┘
       ↑ MudDrawer (collapsible)
```

**NavLink behaviour:** Uses `<MudNavLink>` with `Match="NavLinkMatch.Prefix"` so the active
section remains highlighted for all routes under its prefix.

**MudBlazor providers** registered at layout root: `MudThemeProvider`, `MudPopoverProvider`,
`MudDialogProvider`, `MudSnackbarProvider`.

---

## 5. Page Specifications

### 5.1 MemberList — `/members`

**File:** `src/Client/Components/Pages/Members/MemberList.razor`
**Render mode:** `@rendermode InteractiveServer`
**Injected services:** `ListMembersHandler`, `DeleteMemberHandler`, `IDialogService`

#### Purpose
Primary Members landing page. Displays all members in a paginated table with actions to
navigate to the create form, edit an individual member, or delete a member after confirmation.

#### Layout

#### Layout

Uses `MudTable` with a built-in `MudTablePager` for client-side pagination.

```
Members                                        [Add Member]
─────────────────────────────────────────────────────────
Name             Email                 Created      Actions
─────────────────────────────────────────────────────────
Alice Smith      alice@example.com     09 Mar 2026  ✏  🗑
Bob Jones        bob@example.com       08 Mar 2026  ✏  🗑
...
─────────────────────────────────────────────────────────
                              Rows per page: 20 ▼  1-20 of N  < >
```

#### Behaviours

| Trigger | Behaviour |
|---------|----------|
| Page loads | `OnInitializedAsync` calls `ListMembersHandler` (skip=0, take=500), loads all into `MudTable` |
| Pagination | Handled entirely by `MudTablePager` client-side |
| Add Member button | `MudButton` with `Href="/members/create"` |
| Edit button (row) | `MudIconButton` (Edit icon) with `Href="/members/{id}/edit"` |
| Delete button (row) | `MudIconButton` (Delete icon, Error colour); opens `ConfirmDialog` via `IDialogService.ShowAsync<ConfirmDialog>()` |
| Dialog: Confirm | Calls `DeleteMemberHandler`, reloads list on success |
| Dialog: Cancel | Dialog closed; no action |
| Delete fails | Sets `_errorMessage`; shows `MudAlert` below table |
| No members exist | Shows empty state with `MudLink` to `/members/create` |
| Loading | Shows `MudProgressCircular` until `OnInitializedAsync` completes |

#### Pagination

- All members are loaded in a single query (take=500) and pagination is managed client-side by `MudTablePager`.
- Default rows-per-page: **20**.

#### State

```csharp
private List<MemberDto> _members = [];
private bool _loading = true;
private string? _errorMessage;
```

---

### 5.2 MemberCreate — `/members/create`

**File:** `src/Client/Components/Pages/Members/MemberCreate.razor`
**Render mode:** `@rendermode InteractiveServer`
**Injected services:** `CreateMemberHandler`, `NavigationManager`

#### Purpose
Form page for adding a new member. Uses the shared `MemberForm` component for the input
fields and handles submission, success navigation, and domain error display.

#### Layout

```
Add Member
──────────────────────────────
Name *
[                            ]

Email *
[                            ]

[Save]  Cancel
```

#### Behaviours

| Trigger | Behaviour |
|---------|-----------|
| Page loads | Renders empty `MemberFormModel`; no data fetch required |
| Form submitted (valid) | Calls `CreateMemberHandler` |
| Success | Navigates to `/members` via `NavigationManager.NavigateTo` |
| `DuplicateEmailException` | Shows inline message: "A member with email '{email}' already exists." |
| `ArgumentException` | Shows inline message with exception text |
| Cancel link | Links to `/members` (no async action needed) |
| Save button | Displays "Saving..." label and becomes disabled while request is in flight |

#### State

```csharp
private readonly MemberFormModel _model = new();
private string? _errorMessage;
private bool _submitting;
private bool _ready;       // guards against pre-render; set true in OnAfterRenderAsync
```

**Pre-render guard:** `MemberCreate` uses `OnAfterRenderAsync(firstRender)` to set `_ready = true` and call `StateHasChanged()`. The form is hidden (`MudProgressCircular` shown) until `_ready` is true, preventing interactive component hydration issues.

---

### 5.3 MemberEdit — `/members/{Id:guid}/edit`

**File:** `src/Client/Components/Pages/Members/MemberEdit.razor`
**Render mode:** `@rendermode InteractiveServer`
**Injected services:** `GetMemberByIdHandler`, `UpdateMemberHandler`, `NavigationManager`

#### Purpose
Form page for updating an existing member. Loads the current values on initialisation,
pre-populates the shared `MemberForm` component, and handles submission outcomes.

#### Layout

```
Edit Member
──────────────────────────────
Name *
[Alice Smith                 ]

Email *
[alice@example.com           ]

[Save]  Cancel
```

#### Behaviours

| Trigger | Behaviour |
|---------|-----------|
| Page loads | `OnInitializedAsync` calls `GetMemberByIdHandler` with route `Id` |
| Member found | Populates `MemberFormModel` with current `Name` and `Email` |
| Member not found | Shows "Member Not Found" heading + "Back to Members" link; hides form |
| Loading | Shows "Loading..." until `OnInitializedAsync` completes |
| Form submitted (valid) | Calls `UpdateMemberHandler` |
| Success | Navigates to `/members` |
| Handler returns null | Transitions to "Member Not Found" state (deleted concurrently) |
| `DuplicateEmailException` | Shows inline message: "A member with email '{email}' already exists." |
| `ArgumentException` | Shows inline message with exception text |
| Cancel link | Links to `/members` |
| Save button | Displays "Saving..." and becomes disabled while request is in flight |

#### Route Parameter

```csharp
[Parameter] public Guid Id { get; set; }
```

#### State

```csharp
private MemberFormModel? _model;
private bool _notFound;
private string? _errorMessage;
private bool _submitting;
private bool _ready;       // guards against pre-render; set true in OnAfterRenderAsync
```

**Pre-render guard:** same `OnAfterRenderAsync` pattern as `MemberCreate`.

---

## 6. Shared Component Specifications

### 6.1 MemberFormModel

**File:** `src/Client/Components/Pages/Members/Shared/MemberFormModel.cs`
**Namespace:** `Client.Components.Pages.Members.Shared`

A plain C# class decorated with `System.ComponentModel.DataAnnotations` attributes.
Used as the `EditForm` model in `MemberForm.razor` and instantiated in both Create and Edit pages.

| Property | Type | Validation |
|----------|------|-----------|
| `Name`  | `string` | Required; max 200 chars |
| `Email` | `string` | Required; email format; max 200 chars |

---

### 6.2 MemberForm

**File:** `src/Client/Components/Pages/Members/Shared/MemberForm.razor`

A reusable form component using MudBlazor input controls. Used by both `MemberCreate.razor`
and `MemberEdit.razor` to avoid duplication.

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Model` | `MemberFormModel` | Yes | Bound form model |
| `OnValidSubmit` | `EventCallback<MemberFormModel>` | Yes | Raised when form passes validation |
| `ErrorMessage` | `string?` | No | Domain/server error shown above buttons |
| `IsSubmitting` | `bool` | No | Disables submit button and shows "Saving..." label |

#### Behaviour

- Wraps fields in an `EditForm` with `FormName="member-form"` targeting `Model`
- Registers `DataAnnotationsValidator`
- Uses `MudTextField` with `For` binding for inline per-field validation messages (MudBlazor handles display)
- When `ErrorMessage` is non-null, renders a `MudAlert` (Severity.Error) above the buttons
- Save button is a `MudButton` (Filled, Primary); text: "Save" / "Saving..."; `Disabled` when `IsSubmitting == true`
- Cancel is a `MudButton` (Text variant) with `Href="/members"` — no callback required

---

### 6.3 DeleteConfirmDialog (unused stub)

**File:** `src/Client/Components/Pages/Members/Shared/DeleteConfirmDialog.razor`

This file exists as an empty stub (`@namespace ClubMonitor.Unused`) and is **not used**.
Delete confirmation is instead handled by the shared `ConfirmDialog` component via
MudBlazor's `IDialogService`.

### 6.4 ConfirmDialog (shared)

**File:** `src/Client/Components/Shared/ConfirmDialog.razor`

A generic MudBlazor dialog used for all destructive confirmations across the application.
Invoked via `IDialogService.ShowAsync<ConfirmDialog>(title, parameters)`.

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `ContentText` | `string` | Yes | Body text shown inside the dialog |
| `ConfirmText` | `string` | No | Label for the confirm button (default: "Confirm") |

#### Layout

```
┌──────────────────────────────────────────────────────────┐
│  Delete Member                                           │  ← dialog title
│  Are you sure you want to delete Alice Smith?            │
│  This cannot be undone.                                  │
│                                                 [Cancel] [Delete] │
└──────────────────────────────────────────────────────────┘
```

- Confirm button: Error colour, Filled variant
- Cancel returns `DialogResult.Cancel()`; Confirm returns `DialogResult.Ok(true)`
- `MemberList` awaits `dialog.Result` and only proceeds with deletion when `result.Canceled == false`

---

## 7. Global _Imports.razor Changes

The following `@using` directives are present in
`src/Client/Components/_Imports.razor`:

```razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.JSInterop
@using MudBlazor
@using Client.Components.Shared
@using ClubMonitor.Application.Members
@using ClubMonitor.Domain.Members
@using Client.Components.Pages.Members.Shared
@using ClubMonitor.Application.Clubs
@using ClubMonitor.Domain.Clubs
```

| Using | Provides |
|-------|----------|
| `Microsoft.AspNetCore.Components.Web` | Core web component types |
| `static Microsoft.AspNetCore.Components.Web.RenderMode` | Exposes `InteractiveServer` as an unqualified identifier for `@rendermode` directives — required by the Blazor compiler |
| `Microsoft.AspNetCore.Components.Forms` | `EditForm`, `DataAnnotationsValidator` |
| `MudBlazor` | All MudBlazor components (`MudTable`, `MudTextField`, `MudButton`, `MudAlert`, `IDialogService`, etc.) |
| `Client.Components.Shared` | `ConfirmDialog` (generic shared dialog) |
| `ClubMonitor.Application.Members` | All five handler types + `MemberDto` + command/query records |
| `ClubMonitor.Domain.Members` | `DuplicateEmailException` |
| `Client.Components.Pages.Members.Shared` | `MemberFormModel`, `MemberForm` |
| `ClubMonitor.Application.Clubs` | Club handler types (for Clubs feature) |
| `ClubMonitor.Domain.Clubs` | Club domain types (for Clubs feature) |

## 7a. Client.csproj Project References

Because Blazor components inject Application layer handlers directly, the `Client` project
must reference `Application` and `Domain` at compile time:

```xml
<ItemGroup>
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Domain\Domain.csproj" />
</ItemGroup>
```

These are in addition to the existing `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.

---

## 8. Render Mode Strategy

| Component | Render Mode | Justification |
|-----------|-------------|---------------|
| `MemberList.razor` | `@rendermode InteractiveServer` | Delete confirmation, list refresh, and dynamic error state require SignalR interactivity |
| `MemberCreate.razor` | `@rendermode InteractiveServer` | Form validation feedback and async submission state |
| `MemberEdit.razor` | `@rendermode InteractiveServer` | On-init data load, form validation, async submission |
| `MemberForm.razor` | Inherited from parent | Child components inherit the render mode of their parent |
| `ConfirmDialog.razor` | Opened via `IDialogService` | MudBlazor modal — hosted inside an Interactive Server circuit |
| `MainLayout.razor` | Interactive Server (drawer toggle) | `MudDrawer` open/close state requires interactivity |

---

## 9. Error Handling Summary

| Error Scenario | Component | Handling |
|---------------|-----------|---------|
| Duplicate email on create | `MemberCreate.razor` | Catches `DuplicateEmailException`; passes message to `MemberForm` via `ErrorMessage` parameter |
| Duplicate email on update | `MemberEdit.razor` | Same pattern as create |
| Invalid email format (domain) | `MemberCreate/Edit.razor` | Catches `ArgumentException` from `Email.Create()` |
| Member not found on edit load | `MemberEdit.razor` | Sets `_notFound = true`; hides form, shows informational message |
| Member deleted concurrently during edit | `MemberEdit.razor` | Handler returns `null`; sets `_notFound = true` |
| Delete fails (member not found) | `MemberList.razor` | Handler returns `false`; sets `_errorMessage` shown above table |
| Client-side validation failure | `MemberForm.razor` | `DataAnnotationsValidator` prevents `OnValidSubmit` from firing; `ValidationMessage` shown per field |

---

## 10. Data Flow Diagram

```
User browser
    │ (SignalR WebSocket)
    ▼
MemberList / MemberCreate / MemberEdit (Blazor Server component)
    │ (constructor injection via DI)
    ▼
ListMembersHandler / CreateMemberHandler / etc. (Application layer)
    │ (constructor injection via DI)
    ▼
IMemberRepository (Domain interface)
    │ (resolved to)
    ▼
MemberRepository (Infrastructure — internal sealed class)
    │ (EF Core DbContext)
    ▼
AppDbContext → PostgreSQL  (table: members)
```

---

## 11. Routing Summary

| Route | Component | Purpose |
|-------|-----------|---------|
| `/members` | `MemberList.razor` | Paginated list of all members |
| `/members/create` | `MemberCreate.razor` | Add a new member |
| `/members/{Id:guid}/edit` | `MemberEdit.razor` | Edit an existing member |

---

## 12. Constraints and Assumptions

- **MudBlazor** is the component library used throughout. Standard HTML form controls are not used directly; all inputs, buttons, dialogs, tables, and layout are MudBlazor components.
- The `MemberDto` type (`Guid Id`, `string Name`, `string Email`, `DateTimeOffset CreatedAt`) is defined in `GetMemberByIdHandler.cs` and shared across all handlers.
- Pagination is client-side via `MudTablePager`. All members are loaded in a single query (take=500); server-side paging was not implemented.
- Search and sort are not in scope for this iteration.
- The Members UI does not expose Club Membership management (`/api/clubs/{id}/members`). That is a separate feature belonging to the Clubs UI slice.
- `DeleteConfirmDialog.razor` in `Pages/Members/Shared/` is an unused stub; the active shared dialog is `src/Client/Components/Shared/ConfirmDialog.razor`.
