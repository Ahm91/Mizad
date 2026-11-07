using System.Reflection;
using System.Xml.Linq;

namespace Mizad.UnitTests.Architecture;

/// <summary>
/// Enforces the dependency rules of ARCHITECTURE §1 (D-001) on both compiled references and project files.
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Mizad.Domain.Common.Result).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Mizad.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Mizad.Infrastructure.DependencyInjection).Assembly;

    private static readonly string[] WpfAssemblies = ["PresentationFramework", "PresentationCore", "WindowsBase", "System.Xaml"];

    private static readonly string[] ApplicationForbidden =
    [
        "Mizad.Infrastructure", "Mizad", "Microsoft.EntityFrameworkCore.SqlServer", "Microsoft.Data.SqlClient",
        "QuestPDF", "ClosedXML", "Serilog", .. WpfAssemblies,
    ];

    private static readonly string[] InfrastructureForbidden = ["Mizad", .. WpfAssemblies];

    [Fact]
    public void Domain_ReferencesOnlyTheBaseClassLibrary()
    {
        var references = ReferencedAssemblyNames(DomainAssembly);

        references.ShouldAllBe(name => name.StartsWith("System", StringComparison.Ordinal) || name == "netstandard");
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureDesktopSqlServerOrWpf()
    {
        ReferencedAssemblyNames(ApplicationAssembly).Intersect(ApplicationForbidden).ShouldBeEmpty();
    }

    [Fact]
    public void Infrastructure_DoesNotReferenceDesktopOrWpf()
    {
        ReferencedAssemblyNames(InfrastructureAssembly).Intersect(InfrastructureForbidden).ShouldBeEmpty();
    }

    [Fact]
    public void ProjectFiles_RespectLayerDependencies()
    {
        ProjectReferences("Mizad.Domain").ShouldBeEmpty();
        ProjectReferences("Mizad.Application").ShouldBe(["Mizad.Domain"], ignoreOrder: true);
        ProjectReferences("Mizad.Infrastructure").ShouldBe(["Mizad.Application", "Mizad.Domain"], ignoreOrder: true);
        ProjectReferences("Mizad.Desktop").ShouldBe(["Mizad.Application", "Mizad.Domain", "Mizad.Infrastructure"], ignoreOrder: true);
    }

    [Fact]
    public void Domain_HasNoPackageReferences()
    {
        var project = LoadProject("Mizad.Domain");

        project.Descendants("PackageReference").ShouldBeEmpty();
    }

    [Fact]
    public void ProjectFiles_DoNotPinPackageVersions_CentralManagementOnly()
    {
        var projectFiles = Directory.GetFiles(Path.Combine(RepositoryRoot.Path, "src"), "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(RepositoryRoot.Path, "tests"), "*.csproj", SearchOption.AllDirectories));

        foreach (var file in projectFiles)
        {
            var versioned = XDocument.Load(file).Descendants("PackageReference")
                .Where(element => element.Attribute("Version") is not null);
            versioned.ShouldBeEmpty($"{Path.GetFileName(file)} must take package versions from Directory.Packages.props");
        }
    }

    private static string[] ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(name => name.Name ?? string.Empty).ToArray();

    private static string[] ProjectReferences(string projectName) =>
        LoadProject(projectName).Descendants("ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension(element.Attribute("Include")!.Value))
            .ToArray();

    private static XDocument LoadProject(string projectName) =>
        XDocument.Load(Path.Combine(RepositoryRoot.Path, "src", projectName, projectName + ".csproj"));
}
