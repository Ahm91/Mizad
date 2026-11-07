using Microsoft.Extensions.DependencyInjection;

namespace Mizad.Application;

public static class DependencyInjection
{
    /// <summary>Registers Application-layer services. Module services are added here phase by phase.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
