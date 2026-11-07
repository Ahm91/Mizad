namespace Mizad.Application.Abstractions.Configuration;

public enum SqlAuthenticationMode
{
    /// <summary>Windows integrated security (default for a single PC).</summary>
    Windows = 0,

    /// <summary>SQL Server login (workgroup LAN without a domain).</summary>
    SqlServer = 1,
}

/// <summary>
/// Per-machine database connection settings (D-019, D-042), stored in
/// <c>%ProgramData%\Mizad\connection.json</c>. The password is held in memory only;
/// the store persists it DPAPI-encrypted and never as plain text.
/// </summary>
public sealed record ConnectionSettings
{
    public required string Server { get; init; }

    public string Database { get; init; } = "Mizad";

    public SqlAuthenticationMode AuthenticationMode { get; init; } = SqlAuthenticationMode.Windows;

    public string? UserName { get; init; }

    public string? Password { get; init; }

    /// <summary>Accept the server certificate without validation (LAN use; risk documented in D-019).</summary>
    public bool TrustServerCertificate { get; init; } = true;

    public int ConnectTimeoutSeconds { get; init; } = 15;

    /// <summary>Hide the password from logs and debugger displays.</summary>
    public override string ToString() =>
        $"Server={Server}; Database={Database}; Auth={AuthenticationMode}; User={UserName ?? "-"}";
}
