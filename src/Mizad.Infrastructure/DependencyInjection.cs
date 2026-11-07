using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Configuration;
using Mizad.Application.Abstractions.Database;
using Mizad.Application.Abstractions.Persistence;
using Mizad.Infrastructure.Backup;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Persistence;
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

        // Local configuration (Phase 1)
        services.AddSingleton(paths);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISecretProtector, DpapiSecretProtector>();
        services.AddSingleton<IConnectionSettingsStore, ConnectionSettingsStore>();
        services.AddSingleton<IMachineSettingsStore, MachineSettingsStore>();
        services.AddSingleton<IUserPreferencesStore, UserPreferencesStore>();
        services.AddSingleton<IConnectionStringFactory, SqlConnectionStringFactory>();

        // Session placeholder until login exists (replaced in task 3.6).
        services.TryAddSingleton<ICurrentUser, AnonymousCurrentUser>();

        // Persistence (Phase 2)
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        services.AddSingleton<AuditStampInterceptor>();
        services.AddSingleton<IDbContextFactory<MizadDbContext>, MizadDbContextFactory>();
        services.AddSingleton<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ISequenceAllocator, SequenceAllocator>();
        services.AddSingleton<IServerClock, ServerClock>();
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<BackupOptions>();
        services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
