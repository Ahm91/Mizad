using Microsoft.Data.SqlClient;
using Mizad.Application.Abstractions.Configuration;

namespace Mizad.Infrastructure.Configuration;

internal sealed class SqlConnectionStringFactory : IConnectionStringFactory
{
    public const string ApplicationName = "Mizad";

    public string Create(ConnectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Server);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Database);

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = settings.Server,
            InitialCatalog = settings.Database,
            ApplicationName = ApplicationName,
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = settings.TrustServerCertificate,
            ConnectTimeout = settings.ConnectTimeoutSeconds,
            MultipleActiveResultSets = false,
        };

        if (settings.AuthenticationMode == SqlAuthenticationMode.Windows)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.UserName);
            builder.UserID = settings.UserName;
            builder.Password = settings.Password ?? string.Empty;
        }

        return builder.ConnectionString;
    }
}
