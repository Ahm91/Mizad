using System.Security.Cryptography;
using System.Text;

namespace Mizad.Infrastructure.Security;

/// <summary>Protects small secrets (the SQL login password) at rest.</summary>
internal interface ISecretProtector
{
    string Protect(string plainText);

    string Unprotect(string protectedText);
}

/// <summary>
/// Windows DPAPI with <see cref="DataProtectionScope.LocalMachine"/> (D-019): any Windows user of this
/// computer running Mizad can decrypt; copying the file to another computer cannot.
/// </summary>
internal sealed class DpapiSecretProtector : ISecretProtector
{
    private static readonly byte[] Entropy = "Mizad.ConnectionSettings.v1"u8.ToArray();

    public string Protect(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        var cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), Entropy, DataProtectionScope.LocalMachine);
        return Convert.ToBase64String(cipher);
    }

    public string Unprotect(string protectedText)
    {
        ArgumentNullException.ThrowIfNull(protectedText);
        var plain = ProtectedData.Unprotect(Convert.FromBase64String(protectedText), Entropy, DataProtectionScope.LocalMachine);
        return Encoding.UTF8.GetString(plain);
    }
}
