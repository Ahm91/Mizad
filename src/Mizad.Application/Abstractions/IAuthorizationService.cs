using Mizad.Domain.Common;

namespace Mizad.Application.Abstractions;

/// <summary>
/// Central permission check used at the start of every Application service (BR-100, D-017).
/// Hiding UI elements is never a substitute for calling this service.
/// </summary>
public interface IAuthorizationService
{
    bool Has(string permissionCode);

    /// <summary>Returns <see cref="Result.Success()"/> or a <see cref="ErrorKind.Forbidden"/> failure.</summary>
    Result Authorize(string permissionCode);
}
