using ClubMonitor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Infrastructure (EF Core etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Newer Blazor: Razor Components + Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapGet("/api/db/ping", async (ClubMonitor.Infrastructure.Persistence.AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return Results.Ok(new { canConnect });
});

if (app.Environment.IsDevelopment())
{
  //  app.UseSwagger();
  //  app.UseSwaggerUI();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapRazorComponents<Client.Components.App>()
   .AddInteractiveServerRenderMode();

app.Run();