using Mizad.Application.Abstractions.Configuration;
using Mizad.Infrastructure.Security;

namespace Mizad.Infrastructure.Configuration;

/// <summary>
/// Persists <see cref="ConnectionSettings"/> to <c>connection.json</c>. The password is stored only as a
/// DPAPI-protected Base64 value (<c>ProtectedPassword</c>) and never in plain text (D-019).
/// </summary>
internal sealed class ConnectionSettingsStore(MizadPaths paths, ISecretProtector protector) : IConnectionSettingsStore
{
    public bool Exists => File.Exists(paths.ConnectionFile);

    public ConnectionSettings? Load()
    {
        var file = JsonFile.Read<ConnectionSettingsFile>(paths.ConnectionFile);
        if (file is null)
        {
            return null;
        }

        return new ConnectionSettings
        {
            Server = file.Server,
            Database = file.Database,
            AuthenticationMode = file.AuthenticationMode,
            UserName = file.UserName,
            Password = string.IsNullOrEmpty(file.ProtectedPassword) ? null : protector.Unprotect(file.ProtectedPassword),
            TrustServerCertificate = file.TrustServerCertificate,
            ConnectTimeoutSeconds = file.ConnectTimeoutSeconds,
        };
    }

    public void Save(ConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var usesSqlLogin = settings.AuthenticationMode == SqlAuthenticationMode.SqlServer;
        var file = new ConnectionSettingsFile
        {
            Server = settings.Server,
            Database = settings.Database,
            AuthenticationMode = settings.AuthenticationMode,
            UserName = usesSqlLogin ? settings.UserName : null,
            ProtectedPassword = usesSqlLogin && !string.IsNullOrEmpty(settings.Password)
                ? protector.Protect(settings.Password)
                : null,
            TrustServerCertificate = settings.TrustServerCertificate,
            ConnectTimeoutSeconds = settings.ConnectTimeoutSeconds,
        };

        JsonFile.Write(paths.ConnectionFile, file);
    }

    /// <summary>On-disk shape. <see cref="FormatVersion"/> allows future migrations of the file.</summary>
    internal sealed record ConnectionSettingsFile
    {
        public int FormatVersion { get; init; } = 1;

        public required string Server { get; init; }

        public required string Database { get; init; }

        public SqlAuthenticationMode AuthenticationMode { get; init; }

        public string? UserName { get; init; }

        public string? ProtectedPassword { get; init; }

        public bool TrustServerCertificate { get; init; }

        public int ConnectTimeoutSeconds { get; init; }
    }
}
