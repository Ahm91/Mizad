using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mizad.Desktop.Composition;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Logging;
using Serilog;

namespace Mizad.Desktop;

/// <summary>
/// Application entry point. Builds the Generic Host (DI, configuration, Serilog) and shows the main window.
/// The full startup pipeline (connection setup → database → setup wizard → login) arrives in task 4.14.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        GlobalExceptionHandler.Register(this);

        try
        {
            var paths = MizadPaths.CreateDefault();
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                Args = e.Args,
                ApplicationName = "Mizad",
                ContentRootPath = AppContext.BaseDirectory,
            });

            Log.Logger = MizadLogging.CreateLogger(paths, builder.Configuration, AppInfo.Version);
            builder.Logging.ClearProviders();
            builder.Services.AddSerilog(Log.Logger, dispose: false);

            // GUI app: suppress the console lifetime's "Press Ctrl+C" status messages.
            builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
            builder.Services.AddMizad(paths);

            _host = builder.Build();
            await _host.StartAsync();

            Log.Information("Mizad {Version} started on {OS}", AppInfo.Version, Environment.OSVersion);

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = GlobalExceptionHandler.Report(ex, "Startup");
            GlobalExceptionHandler.ShowFriendlyError(reference);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host is not null)
            {
                // Host shutdown does not need the UI thread; blocking briefly here is safe at process exit.
                _host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                _host.Dispose();
            }

            Log.Information("Mizad exited with code {ExitCode}", e.ApplicationExitCode);
        }
        finally
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
