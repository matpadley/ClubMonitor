using ClubMonitor.Application.Members;
using Microsoft.Extensions.DependencyInjection;

namespace ClubMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<GetMemberByIdHandler>();
        return services;
    }
}