using ClubMonitor.Application.Clubs;
using ClubMonitor.Application.Cups;
using ClubMonitor.Application.Fixtures;
using ClubMonitor.Application.Leagues;
using ClubMonitor.Application.Members;
using ClubMonitor.Application.UserProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace ClubMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Members
        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<GetMemberByIdHandler>();
        services.AddScoped<ListMembersHandler>();
        services.AddScoped<UpdateMemberHandler>();
        services.AddScoped<DeleteMemberHandler>();

        // Clubs
        services.AddScoped<CreateClubHandler>();
        services.AddScoped<GetClubByIdHandler>();
        services.AddScoped<ListClubsHandler>();
        services.AddScoped<UpdateClubHandler>();
        services.AddScoped<DeleteClubHandler>();
        services.AddScoped<AddMemberToClubHandler>();
        services.AddScoped<ListClubMembersHandler>();
        services.AddScoped<ListMemberClubsHandler>();
        services.AddScoped<RemoveMemberFromClubHandler>();

        // Leagues
        services.AddScoped<CreateLeagueHandler>();
        services.AddScoped<GetLeagueByIdHandler>();
        services.AddScoped<ListLeaguesHandler>();
        services.AddScoped<UpdateLeagueHandler>();
        services.AddScoped<DeleteLeagueHandler>();
        services.AddScoped<AddClubToLeagueHandler>();
        services.AddScoped<ListLeagueClubsHandler>();
        services.AddScoped<RemoveClubFromLeagueHandler>();
        services.AddScoped<GetLeagueStandingsHandler>();

        // Cups
        services.AddScoped<CreateCupHandler>();
        services.AddScoped<GetCupByIdHandler>();
        services.AddScoped<ListCupsHandler>();
        services.AddScoped<UpdateCupHandler>();
        services.AddScoped<DeleteCupHandler>();
        services.AddScoped<AddClubToCupHandler>();
        services.AddScoped<ListCupClubsHandler>();
        services.AddScoped<RemoveClubFromCupHandler>();
        services.AddScoped<DrawCupHandler>();

        // Fixtures
        services.AddScoped<CreateFixtureHandler>();
        services.AddScoped<GetFixtureByIdHandler>();
        services.AddScoped<ListFixturesByCompetitionHandler>();
        services.AddScoped<RecordResultHandler>();
        services.AddScoped<RescheduleFixtureHandler>();

        // User Profiles
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<GetUserProfileByIdHandler>();
        services.AddScoped<UpdateUserProfileHandler>();

        return services;
    }
}