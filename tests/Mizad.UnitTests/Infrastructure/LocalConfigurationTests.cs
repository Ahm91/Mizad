using Mizad.Application.Abstractions.Configuration;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Security;

namespace Mizad.UnitTests.Infrastructure;

/// <summary>Local configuration files (D-019, D-042): round-trips and secret protection.</summary>
public sealed class LocalConfigurationTests : IDisposable
{
    private const string SecretPassword = "Sup3r-Secret!كلمة";

    private readonly string _root = Path.Combine(Path.GetTempPath(), "MizadTests", Guid.NewGuid().ToString("N"));
    private readonly MizadPaths _paths;

    public LocalConfigurationTests()
    {
        _paths = new MizadPaths(Path.Combine(_root, "machine"), Path.Combine(_root, "user"));
    }

    [Fact]
    public void ConnectionSettings_SqlLogin_RoundTripsAndNeverStoresPlainPassword()
    {
        var store = new ConnectionSettingsStore(_paths, new DpapiSecretProtector());
        var settings = new ConnectionSettings
        {
            Server = @"SERVER-PC\MIZAD",
            Database = "Mizad",
            AuthenticationMode = SqlAuthenticationMode.SqlServer,
            UserName = "mizad_app",
            Password = SecretPassword,
        };

        store.Save(settings);

        store.Exists.ShouldBeTrue();
        File.ReadAllText(_paths.ConnectionFile).ShouldNotContain(SecretPassword);
        File.ReadAllText(_paths.ConnectionFile).ShouldContain("ProtectedPassword");
        store.Load().ShouldBe(settings);
    }

    [Fact]
    public void ConnectionSettings_WindowsAuth_DropsSqlCredentials()
    {
        var store = new ConnectionSettingsStore(_paths, new DpapiSecretProtector());

        store.Save(new ConnectionSettings { Server = ".", UserName = "ignored", Password = "ignored" });

        var loaded = store.Load().ShouldNotBeNull();
        loaded.AuthenticationMode.ShouldBe(SqlAuthenticationMode.Windows);
        loaded.UserName.ShouldBeNull();
        loaded.Password.ShouldBeNull();
    }

    [Fact]
    public void ConnectionSettings_Missing_ReturnsNull()
    {
        var store = new ConnectionSettingsStore(_paths, new DpapiSecretProtector());

        store.Exists.ShouldBeFalse();
        store.Load().ShouldBeNull();
    }

    [Fact]
    public void ConnectionSettings_CorruptedFile_ThrowsInvalidData()
    {
        Directory.CreateDirectory(_paths.MachineDataDirectory);
        File.WriteAllText(_paths.ConnectionFile, "{ not json");
        var store = new ConnectionSettingsStore(_paths, new DpapiSecretProtector());

        Should.Throw<InvalidDataException>(() => store.Load());
    }

    [Fact]
    public void ConnectionSettings_ToString_HidesPassword()
    {
        var settings = new ConnectionSettings { Server = ".", Password = SecretPassword };

        settings.ToString().ShouldNotContain(SecretPassword);
    }

    [Fact]
    public void Dpapi_ProtectedValueDiffersFromPlainAndRoundTrips()
    {
        var protector = new DpapiSecretProtector();

        var protectedValue = protector.Protect(SecretPassword);

        protectedValue.ShouldNotContain(SecretPassword);
        protector.Unprotect(protectedValue).ShouldBe(SecretPassword);
    }

    [Fact]
    public void MachineAndUserSettings_DefaultWhenMissing_AndRoundTrip()
    {
        var machineStore = new MachineSettingsStore(_paths);
        var userStore = new UserPreferencesStore(_paths);

        machineStore.Load().ShouldBe(new MachineSettings());
        userStore.Load().ShouldBe(new UserPreferences());

        var machine = new MachineSettings { DefaultWarehouseId = 2, ReceiptPrinterName = "طابعة الكاشير" };
        var user = new UserPreferences { LastUserName = "admin", SidebarCollapsed = true };
        machineStore.Save(machine);
        userStore.Save(user);

        machineStore.Load().ShouldBe(machine);
        userStore.Load().ShouldBe(user);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
