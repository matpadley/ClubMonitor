using ClubMonitor.Application;
using ClubMonitor.Application.Members;
using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapRazorComponents<Client.Components.App>()
   .AddInteractiveServerRenderMode();

app.Run();

// Expose Program to the integration test project
public partial class Program { }