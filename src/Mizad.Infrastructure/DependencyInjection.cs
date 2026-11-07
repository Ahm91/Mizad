using Microsoft.Extensions.DependencyInjection;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Configuration;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Security;
using Mizad.Infrastructure.Time;

namespace Mizad.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers Infrastructure implementations. Called only from the Desktop composition root.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, MizadPaths paths)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(paths);

        services.AddSingleton(paths);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISecretProtector, DpapiSecretProtector>();
        services.AddSingleton<IConnectionSettingsStore, ConnectionSettingsStore>();
        services.AddSingleton<IMachineSettingsStore, MachineSettingsStore>();
        services.AddSingleton<IUserPreferencesStore, UserPreferencesStore>();
        services.AddSingleton<IConnectionStringFactory, SqlConnectionStringFactory>();

        return services;
    }
}
