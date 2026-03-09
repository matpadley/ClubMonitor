# Members UI — Specification

**Date:** 2026-03-09
**Technology:** Blazor SSR with Interactive Server render mode (.NET 10)
**Status:** Approved for implementation

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

The `src/Api` project hosts Blazor SSR via `MapRazorComponents<Client.Components.App>()` with
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
├── App.razor                                    (existing — unchanged)
├── _Imports.razor                               (updated — new usings added)
├── Layout/
│   └── MainLayout.razor                         (updated — navigation sidebar added)
└── Pages/
    └── Members/
        ├── MemberList.razor                     @page "/members"
        ├── MemberCreate.razor                   @page "/members/create"
        ├── MemberEdit.razor                     @page "/members/{Id:guid}/edit"
        └── Shared/
            ├── MemberFormModel.cs               (form model with DataAnnotations)
            ├── MemberForm.razor                 (shared form fields component)
            └── DeleteConfirmDialog.razor        (inline delete confirmation overlay)

docs/
└── members-ui-specification.md                  (this document)
```

---

## 4. Navigation — MainLayout.razor

`MainLayout.razor` is updated to include a left sidebar navigation. The sidebar lists all top-
level sections of the application. The Members link is the first entry; future sections (Clubs,
Leagues, Cups) are expected to follow the same pattern.

```
┌─────────────────────────────────────────────────────────┐
│  ClubMonitor                                            │
├───────────────┬─────────────────────────────────────────┤
│  Members      │  <page content>                         │
│  Clubs        │                                         │
│  Leagues      │                                         │
│  Cups         │                                         │
└───────────────┴─────────────────────────────────────────┘
```

**NavLink behaviour:** Uses `<NavLink>` with `Match="NavLinkMatch.Prefix"` so the Members links
remains highlighted for all routes under `/members`.

---

## 5. Page Specifications

### 5.1 MemberList — `/members`

**File:** `src/Client/Components/Pages/Members/MemberList.razor`
**Render mode:** `@rendermode InteractiveServer`
**Injected services:** `ListMembersHandler`, `DeleteMemberHandler`

#### Purpose
Primary Members landing page. Displays all members in a paginated table with actions to
navigate to the create form, edit an individual member, or delete a member after confirmation.

#### Layout

```
Members                                        [Add Member]
─────────────────────────────────────────────────────────
Name             Email                 Created      Actions
─────────────────────────────────────────────────────────
Alice Smith      alice@example.com     09 Mar 2026  Edit  Delete
Bob Jones        bob@example.com       08 Mar 2026  Edit  Delete
...
─────────────────────────────────────────────────────────
                                 [Previous]  Page 1  [Next]
```

#### Behaviours

| Trigger | Behaviour |
|---------|-----------|
| Page loads | `OnInitializedAsync` calls `ListMembersHandler` (skip=0, take=20) |
| Next button | Increments skip by page size (20), reloads list |
| Previous button | Decrements skip by page size (min 0), reloads list |
| Add Member button | Navigates to `/members/create` |
| Edit button (row) | Navigates to `/members/{id}/edit` |
| Delete button (row) | Sets `_memberToDelete`, shows `DeleteConfirmDialog` |
| Dialog: Confirm | Calls `DeleteMemberHandler`, reloads list on success |
| Dialog: Cancel | Clears `_memberToDelete`, hides dialog |
| Delete fails | Shows inline error message "Member could not be deleted." |
| No members exist | Shows empty state: "No members yet. Add the first one." with a link |
| Loading | Shows "Loading..." text until first data load completes |

#### Pagination

- Page size: **20** records
- Previous button disabled when `_skip == 0`
- Next button disabled when the returned list has fewer than 20 records
- Page indicator shows current page number: `Page {_skip / PageSize + 1}`

#### State

```csharp
private const int PageSize = 20;
private List<MemberDto> _members = [];
private bool _loading = true;
private int _skip = 0;
private MemberDto? _memberToDelete;
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
```

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
```

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

A reusable form component encapsulating the `EditForm`, `DataAnnotationsValidator`,
`ValidationSummary`, input fields, error display, and action buttons. Used by both
`MemberCreate.razor` and `MemberEdit.razor` to avoid duplication.

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Model` | `MemberFormModel` | Yes | Bound form model |
| `OnValidSubmit` | `EventCallback<MemberFormModel>` | Yes | Raised when form passes validation |
| `ErrorMessage` | `string?` | No | Domain/server error shown above buttons |
| `IsSubmitting` | `bool` | No | Disables submit button and shows "Saving..." label |

#### Behaviour

- Wraps fields in an `EditForm` targeting `Model`
- Registers `DataAnnotationsValidator`
- Displays `ValidationSummary` at the top of the form
- Shows `ValidationMessage` inline beneath each field
- Raises `OnValidSubmit` only when all `DataAnnotationsValidator` rules pass
- When `ErrorMessage` is non-null, displays it as a prominent error block above buttons
- Save button text: "Save" normally, "Saving..." when `IsSubmitting == true`
- Save button is `disabled` when `IsSubmitting == true`
- Cancel is a plain `<a href="/members">Cancel</a>` link requiring no callback

---

### 6.3 DeleteConfirmDialog

**File:** `src/Client/Components/Pages/Members/Shared/DeleteConfirmDialog.razor`

An inline confirmation overlay rendered directly within `MemberList.razor` when a delete
action is initiated. Displays the member's name and prompts for explicit confirmation.

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `MemberName` | `string` | Yes | Member's name shown in the prompt |
| `OnConfirm` | `EventCallback` | Yes | Raised when Delete button is clicked |
| `OnCancel` | `EventCallback` | Yes | Raised when Cancel button is clicked |

#### Layout

```
┌──────────────────────────────────────────────────────────┐
│  Are you sure you want to delete Alice Smith?            │
│  This cannot be undone.                                  │
│                                                          │
│                          [Delete]  [Cancel]              │
└──────────────────────────────────────────────────────────┘
```

Rendered as a fixed-position overlay with a semi-transparent backdrop. No JavaScript required;
fully managed via Blazor component state in `MemberList.razor`.

---

## 7. Global _Imports.razor Changes

The following `@using` directives are added to
`src/Client/Components/_Imports.razor` to make all required types available globally
to every `.razor` file:

```razor
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Forms
@using ClubMonitor.Application.Members
@using ClubMonitor.Domain.Members
@using Client.Components.Pages.Members.Shared
```

| Using | Provides |
|-------|----------|
| `Microsoft.AspNetCore.Components.Web` | Core web component types |
| `static Microsoft.AspNetCore.Components.Web.RenderMode` | Exposes `InteractiveServer` as an unqualified identifier for `@rendermode` directives — required by the Blazor compiler |
| `Microsoft.AspNetCore.Components.Forms` | `EditForm`, `InputText`, `DataAnnotationsValidator`, `ValidationMessage` |
| `ClubMonitor.Application.Members` | All five handler types + `MemberDto` + command/query records |
| `ClubMonitor.Domain.Members` | `DuplicateEmailException` |
| `Client.Components.Pages.Members.Shared` | `MemberFormModel`, `MemberForm`, `DeleteConfirmDialog` |

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
| `MemberList.razor` | `@rendermode InteractiveServer` | Pagination, delete confirmation, and list refresh require SignalR interactivity |
| `MemberCreate.razor` | `@rendermode InteractiveServer` | Form validation feedback and async submission state |
| `MemberEdit.razor` | `@rendermode InteractiveServer` | On-init data load, form validation, async submission |
| `MemberForm.razor` | Inherited from parent | Child components inherit the render mode of their parent |
| `DeleteConfirmDialog.razor` | Inherited from parent | Rendered inside `MemberList` which is Interactive Server |
| `MainLayout.razor` | Static SSR | No interactivity required in the shell layout |

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

- No CSS framework is introduced. Components use semantic HTML and minimal inline styles.
  Styling beyond basic structure is deferred to a future design/theming task.
- The `MemberDto` type (`Guid Id`, `string Name`, `string Email`, `DateTimeOffset CreatedAt`)
  is defined in `GetMemberByIdHandler.cs` and already shared across all handlers.
- Page size is fixed at 20. A configurable page size selector is not in scope.
- Search and sort are not in scope for this iteration.
- The Members UI does not expose Club Membership management (`/api/clubs/{id}/members`).
  That is a separate feature belonging to the Clubs UI slice.
