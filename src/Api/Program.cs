using System.Text.Json.Serialization;
using Api.Routes;
using ClubMonitor.Application;
using ClubMonitor.Infrastructure;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// In development register developer-only services like Swagger/OpenAPI and a stub authentication
// handler to make local development and integration testing easier. These are intentionally
// omitted in staging/production.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMudServices();

// Register stub auth only in Development alongside Swagger (see above)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Stub";
    }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Client.Components.Shared.StubAuthenticationHandler>("Stub", null);

    builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, Client.Components.Shared.StubAuthenticationStateProvider>();
    builder.Services.AddAuthorizationCore();
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAntiforgery();

app.MapHealthRoutes();
app.MapMembersRoutes();
app.MapClubsRoutes();
app.MapClubMembershipsRoutes();
app.MapLeaguesRoutes();
app.MapCupsRoutes();
app.MapFixturesRoutes();
app.MapUserProfilesRoutes();
app.MapStaticAssets();


app.MapRazorComponents<Api.Components.App>()
   .AddInteractiveServerRenderMode()
   .AddAdditionalAssemblies(typeof(Client.Components.Layout.MainLayout).Assembly);

app.Run();

// Expose Program to the integration test project
namespace Api
{
    public partial class Program { }
}

