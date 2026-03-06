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

        return services;
    }
}