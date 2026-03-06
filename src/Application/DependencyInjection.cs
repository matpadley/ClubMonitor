using Microsoft.Extensions.DependencyInjection;

namespace ClubMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // register use cases
        return services;
    }
}