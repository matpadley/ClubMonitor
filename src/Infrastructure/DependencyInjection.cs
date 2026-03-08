using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;
using ClubMonitor.Domain.Fixtures;
using ClubMonitor.Domain.Leagues;
using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure.Clubs;
using ClubMonitor.Infrastructure.Cups;
using ClubMonitor.Infrastructure.Fixtures;
using ClubMonitor.Infrastructure.Leagues;
using ClubMonitor.Infrastructure.Members;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClubMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("ClubMonitor");

        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:ClubMonitor");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(cs));

        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubMembershipRepository, ClubMembershipRepository>();
        services.AddScoped<ILeagueRepository, LeagueRepository>();
        services.AddScoped<ICupRepository, CupRepository>();
        services.AddScoped<IFixtureRepository, FixtureRepository>();

        return services;
    }
}