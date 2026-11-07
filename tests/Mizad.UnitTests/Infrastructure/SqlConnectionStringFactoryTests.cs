using Microsoft.Data.SqlClient;
using Mizad.Application.Abstractions.Configuration;
using Mizad.Infrastructure.Configuration;

namespace Mizad.UnitTests.Infrastructure;

public sealed class SqlConnectionStringFactoryTests
{
    private readonly SqlConnectionStringFactory _factory = new();

    [Fact]
    public void WindowsAuthentication_UsesIntegratedSecurity()
    {
        var builder = new SqlConnectionStringBuilder(_factory.Create(new ConnectionSettings { Server = ".", Database = "Mizad" }));

        builder.DataSource.ShouldBe(".");
        builder.InitialCatalog.ShouldBe("Mizad");
        builder.IntegratedSecurity.ShouldBeTrue();
        builder.UserID.ShouldBeEmpty();
        builder.ApplicationName.ShouldBe("Mizad");
        builder.Encrypt.ShouldBe(SqlConnectionEncryptOption.Mandatory);
        builder.TrustServerCertificate.ShouldBeTrue();
    }

    [Fact]
    public void SqlAuthentication_UsesLoginAndPassword()
    {
        var settings = new ConnectionSettings
        {
            Server = "SERVER-PC",
            AuthenticationMode = SqlAuthenticationMode.SqlServer,
            UserName = "mizad_app",
            Password = "p@ss",
            ConnectTimeoutSeconds = 30,
        };

        var builder = new SqlConnectionStringBuilder(_factory.Create(settings));

        builder.IntegratedSecurity.ShouldBeFalse();
        builder.UserID.ShouldBe("mizad_app");
        builder.Password.ShouldBe("p@ss");
        builder.ConnectTimeout.ShouldBe(30);
    }

    [Fact]
    public void SqlAuthentication_WithoutUserName_Throws()
    {
        var settings = new ConnectionSettings { Server = ".", AuthenticationMode = SqlAuthenticationMode.SqlServer };

        Should.Throw<ArgumentException>(() => _factory.Create(settings));
    }

    [Fact]
    public void EmptyServer_Throws()
    {
        Should.Throw<ArgumentException>(() => _factory.Create(new ConnectionSettings { Server = " " }));
    }
}
