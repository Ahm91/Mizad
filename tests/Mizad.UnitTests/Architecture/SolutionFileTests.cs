using System.Text.Json;
using System.Xml.Linq;

namespace Mizad.UnitTests.Architecture;

/// <summary>
/// Guards the solution so Visual Studio always starts the WPF application (D-002). Without
/// <c>DefaultStartup</c>, Visual Studio 2026 picks the alphabetically first project — <c>Mizad.Application</c>,
/// a class library — and F5 fails with "A project with an Output Type of Class Library cannot be started directly".
/// </summary>
public sealed class SolutionFileTests
{
    private const string DesktopProject = "src/Mizad.Desktop/Mizad.Desktop.csproj";

    [Fact]
    public void Solution_MarksOnlyTheDesktopApp_AsDefaultStartup()
    {
        var solution = XDocument.Load(Path.Combine(RepositoryRoot.Path, "Mizad.slnx"));

        var startup = solution.Descendants("Project")
            .Where(p => string.Equals((string?)p.Attribute("DefaultStartup"), "true", StringComparison.OrdinalIgnoreCase))
            .Select(p => (string?)p.Attribute("Path"))
            .ToList();

        startup.ShouldBe([DesktopProject]);
    }

    [Fact]
    public void DesktopApp_IsTheOnlyExecutableSourceProject()
    {
        var executables = Directory.GetFiles(Path.Combine(RepositoryRoot.Path, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(file => XDocument.Load(file).Descendants("OutputType").Any(o => o.Value is "Exe" or "WinExe"))
            .Select(file => Path.GetRelativePath(RepositoryRoot.Path, file).Replace('\\', '/'))
            .ToList();

        executables.ShouldBe([DesktopProject]);
    }

    [Fact]
    public void DesktopApp_HasAProjectLaunchProfile()
    {
        var launchSettings = Path.Combine(RepositoryRoot.Path, "src", "Mizad.Desktop", "Properties", "launchSettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(launchSettings));

        var profile = document.RootElement.GetProperty("profiles").GetProperty("Mizad");

        profile.GetProperty("commandName").GetString().ShouldBe("Project");
    }
}
