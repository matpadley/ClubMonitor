using ClubMonitor.Application.Members;
using Microsoft.Extensions.DependencyInjection;

namespace ClubMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateMemberHandler>();
        services.AddScoped<GetMemberByIdHandler>();
        services.AddScoped<ListMembersHandler>();
        services.AddScoped<UpdateMemberHandler>();
        services.AddScoped<DeleteMemberHandler>();
        return services;
    }
}