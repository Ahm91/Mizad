namespace Mizad.UnitTests;

/// <summary>Locates the repository root (the folder containing Mizad.slnx) from the test output directory.</summary>
internal static class RepositoryRoot
{
    public static string Path { get; } = Find();

    private static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "Mizad.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find Mizad.slnx above " + AppContext.BaseDirectory);
    }
}
