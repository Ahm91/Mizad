namespace Mizad.Infrastructure.Configuration;

/// <summary>
/// Well-known file locations (D-042, DEPLOYMENT §7). Overridable for tests.
/// </summary>
public sealed class MizadPaths
{
    public MizadPaths(string machineDataDirectory, string userDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(machineDataDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(userDataDirectory);
        MachineDataDirectory = machineDataDirectory;
        UserDataDirectory = userDataDirectory;
    }

    /// <summary><c>%ProgramData%\Mizad</c> and <c>%LocalAppData%\Mizad</c>.</summary>
    public static MizadPaths CreateDefault() => new(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Mizad"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mizad"));

    /// <summary>Shared by all Windows users of this computer.</summary>
    public string MachineDataDirectory { get; }

    /// <summary>Private to the current Windows user.</summary>
    public string UserDataDirectory { get; }

    public string LogsDirectory => Path.Combine(UserDataDirectory, "logs");

    public string ConnectionFile => Path.Combine(MachineDataDirectory, "connection.json");

    public string MachineSettingsFile => Path.Combine(MachineDataDirectory, "machine.json");

    public string UserPreferencesFile => Path.Combine(UserDataDirectory, "user.json");
}
