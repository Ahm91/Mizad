using System.Text;
using Microsoft.Extensions.Configuration;
using Mizad.Infrastructure.Configuration;
using Serilog;
using Serilog.Events;

namespace Mizad.Infrastructure.Logging;

/// <summary>
/// Serilog setup (ARCHITECTURE §4.7): daily rolling file in <c>%LocalAppData%\Mizad\logs</c>, 30 files kept,
/// machine/version enrichment and masking of secrets. Minimum level from <c>Mizad:Logging:MinimumLevel</c>.
/// </summary>
public static class MizadLogging
{
    public const string MinimumLevelKey = "Mizad:Logging:MinimumLevel";

    public static ILogger CreateLogger(MizadPaths paths, IConfiguration configuration, string appVersion)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(configuration);

        var minimumLevel = Enum.TryParse<LogEventLevel>(configuration[MinimumLevelKey], ignoreCase: true, out var parsed)
            ? parsed
            : LogEventLevel.Information;

        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("AppVersion", appVersion)
            .Destructure.With<SensitiveDataDestructuringPolicy>()
            .WriteTo.File(
                path: Path.Combine(paths.LogsDirectory, "mizad-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                encoding: Encoding.UTF8,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({MachineName} v{AppVersion}) {SourceContext}: {Message:lj}{NewLine}{Exception}",
                formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .CreateLogger();
    }
}
