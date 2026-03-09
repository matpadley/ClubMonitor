using ClubMonitor.Application;
using ClubMonitor.Application.Clubs;
using ClubMonitor.Application.Cups;
using ClubMonitor.Application.Fixtures;
using ClubMonitor.Application.Leagues;
using ClubMonitor.Application.Members;
using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;
using ClubMonitor.Domain.Fixtures;
using ClubMonitor.Domain.Leagues;
using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/db/ping", async (ClubMonitor.Infrastructure.Persistence.AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return Results.Ok(new { canConnect });
});

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// ── Members ──────────────────────────────────────────────────────────────────

app.MapPost("/api/members", async (CreateMemberCommand command, CreateMemberHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/members/{result.Id}", result);
    }
    catch (DuplicateEmailException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/members/{id:guid}", async (Guid id, GetMemberByIdHandler handler, CancellationToken ct) =>
{
    var member = await handler.HandleAsync(new GetMemberByIdQuery(id), ct);
    return member is null ? Results.NotFound() : Results.Ok(member);
});

app.MapGet("/api/members", async (int? skip, int? take, ListMembersHandler handler, CancellationToken ct) =>
{
    const int MaxPageSize = 1000;

    var validatedSkip = skip ?? 0;
    var validatedTake = take ?? 50;

    if (validatedSkip < 0)
        return Results.BadRequest(new { error = "skip must be greater than or equal to 0." });
    if (validatedTake <= 0)
        return Results.BadRequest(new { error = "take must be greater than 0." });
    if (validatedTake > MaxPageSize)
        return Results.BadRequest(new { error = $"take cannot be greater than {MaxPageSize}." });

    var members = await handler.HandleAsync(new ListMembersQuery(validatedSkip, validatedTake), ct);
    return Results.Ok(members);
});

app.MapPut("/api/members/{id:guid}", async (Guid id, UpdateMemberBody body, UpdateMemberHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new UpdateMemberCommand(id, body.Name, body.Email), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (DuplicateEmailException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/members/{id:guid}", async (Guid id, DeleteMemberHandler handler, CancellationToken ct) =>
{
    var deleted = await handler.HandleAsync(new DeleteMemberCommand(id), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

// ── Clubs ─────────────────────────────────────────────────────────────────────

app.MapPost("/api/clubs", async (CreateClubCommand command, CreateClubHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/clubs/{result.Id}", result);
    }
    catch (DuplicateClubNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/clubs/{id:guid}", async (Guid id, GetClubByIdHandler handler, CancellationToken ct) =>
{
    var club = await handler.HandleAsync(new GetClubByIdQuery(id), ct);
    return club is null ? Results.NotFound() : Results.Ok(club);
});

app.MapGet("/api/clubs", async (int? skip, int? take, ListClubsHandler handler, CancellationToken ct) =>
{
    var clubs = await handler.HandleAsync(new ListClubsQuery(skip ?? 0, take ?? 50), ct);
    return Results.Ok(clubs);
});

app.MapPut("/api/clubs/{id:guid}", async (Guid id, UpdateClubBody body, UpdateClubHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new UpdateClubCommand(id, body.Name), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (DuplicateClubNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/clubs/{id:guid}", async (Guid id, DeleteClubHandler handler, CancellationToken ct) =>
{
    var deleted = await handler.HandleAsync(new DeleteClubCommand(id), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

// ── Club Memberships ──────────────────────────────────────────────────────────

app.MapPost("/api/clubs/{clubId:guid}/members", async (
    Guid clubId, AddMemberToClubBody body,
    AddMemberToClubHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new AddMemberToClubCommand(clubId, body.MemberId, body.Role), ct);
        return Results.Created($"/api/clubs/{clubId}/members", result);
    }
    catch (DuplicateMembershipException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/clubs/{clubId:guid}/members", async (
    Guid clubId, int? skip, int? take,
    ListClubMembersHandler handler, CancellationToken ct) =>
{
    var safeSkip = skip is null or < 0 ? 0 : skip.Value;
    var safeTake = take is null or <= 0 ? 50 : take.Value;
    if (safeTake > 100)
    {
        safeTake = 100;
    }

    var members = await handler.HandleAsync(new ListClubMembersQuery(clubId, safeSkip, safeTake), ct);
    return Results.Ok(members);
});

app.MapDelete("/api/clubs/{clubId:guid}/members/{memberId:guid}", async (
    Guid clubId, Guid memberId,
    RemoveMemberFromClubHandler handler, CancellationToken ct) =>
{
    var deleted = await handler.HandleAsync(new RemoveMemberFromClubCommand(clubId, memberId), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

// ── Leagues ───────────────────────────────────────────────────────────────────

app.MapPost("/api/leagues", async (CreateLeagueCommand command, CreateLeagueHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/leagues/{result.Id}", result);
    }
    catch (DuplicateLeagueNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/leagues/{id:guid}", async (Guid id, GetLeagueByIdHandler handler, CancellationToken ct) =>
{
    var league = await handler.HandleAsync(new GetLeagueByIdQuery(id), ct);
    return league is null ? Results.NotFound() : Results.Ok(league);
});

app.MapGet("/api/leagues", async (int? skip, int? take, ListLeaguesHandler handler, CancellationToken ct) =>
{
    var leagues = await handler.HandleAsync(new ListLeaguesQuery(skip ?? 0, take ?? 50), ct);
    return Results.Ok(leagues);
});

app.MapPut("/api/leagues/{id:guid}", async (Guid id, UpdateLeagueBody body, UpdateLeagueHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new UpdateLeagueCommand(id, body.Name), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (DuplicateLeagueNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/leagues/{id:guid}", async (Guid id, DeleteLeagueHandler handler, CancellationToken ct) =>
{
    var deleted = await handler.HandleAsync(new DeleteLeagueCommand(id), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/api/leagues/{leagueId:guid}/clubs", async (
    Guid leagueId, AddClubToLeagueBody body,
    AddClubToLeagueHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new AddClubToLeagueCommand(leagueId, body.ClubId), ct);
        return Results.Created($"/api/leagues/{leagueId}/clubs", result);
    }
    catch (DuplicateLeagueEntryException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/leagues/{leagueId:guid}/clubs", async (
    Guid leagueId, int? skip, int? take,
    ListLeagueClubsHandler handler, CancellationToken ct) =>
{
    var entries = await handler.HandleAsync(new ListLeagueClubsQuery(leagueId, skip ?? 0, take ?? 50), ct);
    return Results.Ok(entries);
});

app.MapDelete("/api/leagues/{leagueId:guid}/clubs/{clubId:guid}", async (
    Guid leagueId, Guid clubId,
    RemoveClubFromLeagueHandler handler, CancellationToken ct) =>
{
    var deleted = await handler.HandleAsync(new RemoveClubFromLeagueCommand(leagueId, clubId), ct);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/leagues/{leagueId:guid}/standings", async (
    Guid leagueId, GetLeagueStandingsHandler handler, CancellationToken ct) =>
{
    var standings = await handler.HandleAsync(new GetLeagueStandingsQuery(leagueId), ct);
    return Results.Ok(standings);
});

// ── Cups ──────────────────────────────────────────────────────────────────────

app.MapPost("/api/cups", async (CreateCupCommand command, CreateCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/cups/{result.Id}", result);
    }
    catch (DuplicateCupNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/cups/{id:guid}", async (Guid id, GetCupByIdHandler handler, CancellationToken ct) =>
{
    var cup = await handler.HandleAsync(new GetCupByIdQuery(id), ct);
    return cup is null ? Results.NotFound() : Results.Ok(cup);
});

app.MapGet("/api/cups", async (int? skip, int? take, ListCupsHandler handler, CancellationToken ct) =>
{
    var cups = await handler.HandleAsync(new ListCupsQuery(skip ?? 0, take ?? 50), ct);
    return Results.Ok(cups);
});

app.MapPut("/api/cups/{id:guid}", async (Guid id, UpdateCupBody body, UpdateCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new UpdateCupCommand(id, body.Name), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (DuplicateCupNameException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/cups/{id:guid}", async (Guid id, DeleteCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var deleted = await handler.HandleAsync(new DeleteCupCommand(id), ct);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidCupStateException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapPost("/api/cups/{cupId:guid}/clubs", async (
    Guid cupId, AddClubToCupBody body,
    AddClubToCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new AddClubToCupCommand(cupId, body.ClubId), ct);
        return Results.Created($"/api/cups/{cupId}/clubs", result);
    }
    catch (DuplicateCupEntryException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (InvalidCupStateException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/cups/{cupId:guid}/clubs", async (
    Guid cupId, int? skip, int? take,
    ListCupClubsHandler handler, CancellationToken ct) =>
{
    var entries = await handler.HandleAsync(new ListCupClubsQuery(cupId, skip ?? 0, take ?? 50), ct);
    return Results.Ok(entries);
});

app.MapDelete("/api/cups/{cupId:guid}/clubs/{clubId:guid}", async (
    Guid cupId, Guid clubId,
    RemoveClubFromCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var deleted = await handler.HandleAsync(new RemoveClubFromCupCommand(cupId, clubId), ct);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidCupStateException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapPost("/api/cups/{cupId:guid}/draw", async (
    Guid cupId, DrawCupHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new DrawCupCommand(cupId), ct);
        return Results.Ok(result);
    }
    catch (InvalidCupStateException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// ── Fixtures ──────────────────────────────────────────────────────────────────

app.MapPost("/api/fixtures", async (CreateFixtureCommand command, CreateFixtureHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/fixtures/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/fixtures/{id:guid}", async (Guid id, GetFixtureByIdHandler handler, CancellationToken ct) =>
{
    var fixture = await handler.HandleAsync(new GetFixtureByIdQuery(id), ct);
    return fixture is null ? Results.NotFound() : Results.Ok(fixture);
});

app.MapGet("/api/fixtures", async (
    string? type, Guid? competitionId, int? skip, int? take,
    ListFixturesByCompetitionHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<CompetitionType>(type, ignoreCase: true, out var competitionType))
        return Results.BadRequest(new { error = "Invalid competition type. Use 'League' or 'Cup'." });
    if (competitionId is null)
        return Results.BadRequest(new { error = "competitionId is required." });

    var fixtures = await handler.HandleAsync(
        new ListFixturesByCompetitionQuery(competitionType, competitionId.Value, skip ?? 0, take ?? 50), ct);
    return Results.Ok(fixtures);
});

app.MapPut("/api/fixtures/{id:guid}/result", async (
    Guid id, RecordResultBody body,
    RecordResultHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new RecordResultCommand(id, body.HomeScore, body.AwayScore), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/api/fixtures/{id:guid}/schedule", async (
    Guid id, RescheduleBody body,
    RescheduleFixtureHandler handler, CancellationToken ct) =>
{
    try
    {
        var result = await handler.HandleAsync(new RescheduleFixtureCommand(id, body.ScheduledAt, body.Venue), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapRazorComponents<Client.Components.App>()
   .AddInteractiveServerRenderMode();

app.Run();

// Expose Program to the integration test project
public partial class Program { }

record UpdateMemberBody(string Name, string Email);
record UpdateClubBody(string Name);
record UpdateLeagueBody(string Name);
record UpdateCupBody(string Name);
record AddMemberToClubBody(Guid MemberId, ClubRole Role);
record AddClubToLeagueBody(Guid ClubId);
record AddClubToCupBody(Guid ClubId);
record RecordResultBody(int HomeScore, int AwayScore);
record RescheduleBody(DateTimeOffset ScheduledAt, string? Venue);
