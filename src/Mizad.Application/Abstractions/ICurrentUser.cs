namespace Mizad.Application.Abstractions;

/// <summary>The signed-in user of this desktop session (implemented in task 3.6).</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? UserName { get; }

    string? FullName { get; }

    bool HasPermission(string permissionCode);
}
