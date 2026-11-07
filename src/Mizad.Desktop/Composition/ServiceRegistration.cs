using Microsoft.Extensions.DependencyInjection;
using Mizad.Application;
using Mizad.Infrastructure;
using Mizad.Infrastructure.Configuration;

namespace Mizad.Desktop.Composition;

/// <summary>
/// Composition root: the only place where Desktop touches Infrastructure (ARCHITECTURE §1).
/// </summary>
internal static class ServiceRegistration
{
    public static IServiceCollection AddMizad(this IServiceCollection services, MizadPaths paths)
    {
        services.AddApplication();
        services.AddInfrastructure(paths);

        services.AddSingleton<MainWindow>();

        return services;
    }
}
