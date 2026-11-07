namespace Mizad.Application.Abstractions.Configuration;

/// <summary>Settings specific to this computer (D-042): <c>%ProgramData%\Mizad\machine.json</c>.</summary>
public sealed record MachineSettings
{
    public int? DefaultWarehouseId { get; init; }

    public int? DefaultCashAccountId { get; init; }

    public string? ReceiptPrinterName { get; init; }

    public string? A4PrinterName { get; init; }
}

/// <summary>Preferences of the local Windows user (D-042): <c>%LocalAppData%\Mizad\user.json</c>.</summary>
public sealed record UserPreferences
{
    /// <summary>"Remember me" stores the user name only — never a password (D-017).</summary>
    public string? LastUserName { get; init; }

    public bool SidebarCollapsed { get; init; }
}

public interface IConnectionSettingsStore
{
    bool Exists { get; }

    ConnectionSettings? Load();

    void Save(ConnectionSettings settings);
}

public interface IMachineSettingsStore
{
    MachineSettings Load();

    void Save(MachineSettings settings);
}

public interface IUserPreferencesStore
{
    UserPreferences Load();

    void Save(UserPreferences preferences);
}

/// <summary>Builds the SQL Server connection string from <see cref="ConnectionSettings"/>.</summary>
public interface IConnectionStringFactory
{
    string Create(ConnectionSettings settings);
}
