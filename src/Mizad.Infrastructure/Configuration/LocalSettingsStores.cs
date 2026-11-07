using Mizad.Application.Abstractions.Configuration;

namespace Mizad.Infrastructure.Configuration;

/// <summary><c>%ProgramData%\Mizad\machine.json</c> — settings of this computer (D-042).</summary>
internal sealed class MachineSettingsStore(MizadPaths paths) : IMachineSettingsStore
{
    public MachineSettings Load() => JsonFile.Read<MachineSettings>(paths.MachineSettingsFile) ?? new MachineSettings();

    public void Save(MachineSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        JsonFile.Write(paths.MachineSettingsFile, settings);
    }
}

/// <summary><c>%LocalAppData%\Mizad\user.json</c> — preferences of the local Windows user (D-042).</summary>
internal sealed class UserPreferencesStore(MizadPaths paths) : IUserPreferencesStore
{
    public UserPreferences Load() => JsonFile.Read<UserPreferences>(paths.UserPreferencesFile) ?? new UserPreferences();

    public void Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        JsonFile.Write(paths.UserPreferencesFile, preferences);
    }
}
