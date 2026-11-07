using Microsoft.Extensions.Configuration;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Mizad.UnitTests.Infrastructure;

public sealed class LoggingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "MizadTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void DestructuringPolicy_MasksSecretProperties()
    {
        var sink = new CollectingSink();
        using (var logger = new LoggerConfiguration()
                   .Destructure.With<SensitiveDataDestructuringPolicy>()
                   .WriteTo.Sink(sink)
                   .CreateLogger())
        {
            logger.Information("Login {@Request}", new { UserName = "admin", Password = "TopSecret1" });
        }

        var rendered = sink.Events.ShouldHaveSingleItem().RenderMessage(System.Globalization.CultureInfo.InvariantCulture);
        rendered.ShouldContain("admin");
        rendered.ShouldContain(SensitiveDataDestructuringPolicy.Mask);
        rendered.ShouldNotContain("TopSecret1");
    }

    [Fact]
    public void DestructuringPolicy_LeavesOrdinaryObjectsToDefaultHandling()
    {
        var sink = new CollectingSink();
        using (var logger = new LoggerConfiguration()
                   .Destructure.With<SensitiveDataDestructuringPolicy>()
                   .WriteTo.Sink(sink)
                   .CreateLogger())
        {
            logger.Information("Item {@Item}", new { Name = "حليب", Quantity = 2 });
        }

        var rendered = sink.Events.ShouldHaveSingleItem().RenderMessage(System.Globalization.CultureInfo.InvariantCulture);
        rendered.ShouldContain("حليب");
        rendered.ShouldNotContain(SensitiveDataDestructuringPolicy.Mask);
    }

    [Fact]
    public void CreateLogger_WritesDailyFileInLogsDirectory()
    {
        var paths = new MizadPaths(Path.Combine(_root, "machine"), Path.Combine(_root, "user"));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [MizadLogging.MinimumLevelKey] = "Debug" })
            .Build();

        var logger = MizadLogging.CreateLogger(paths, configuration, "0.1.0");
        logger.Debug("اختبار السجل");
        ((IDisposable)logger).Dispose();

        var file = Directory.GetFiles(paths.LogsDirectory, "mizad-*.log").ShouldHaveSingleItem();
        var content = File.ReadAllText(file);
        content.ShouldContain("اختبار السجل");
        content.ShouldContain("v0.1.0");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
