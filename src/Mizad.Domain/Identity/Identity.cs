using Mizad.Domain.Common;

namespace Mizad.Domain.Identity;

/// <summary>Application user (DATABASE_DESIGN §3). Exactly one role per user (D-018).</summary>
public class User : AuditableEntity<int>, IHasRowVersion
{
    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash in the versioned format <c>v1$iter$salt$hash</c> (D-017). Never a plain password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>A named set of permission codes. The system administrator role is locked (<see cref="IsSystem"/>).</summary>
public class Role : AuditableEntity<int>, IHasRowVersion
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public ICollection<RolePermissionGrant> Permissions { get; } = [];

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>Permission code granted to a role (codes defined in code — BUSINESS_RULES §9). Table: RolePermissions.</summary>
public class RolePermissionGrant
{
    public int RoleId { get; set; }

    public string PermissionCode { get; set; } = string.Empty;
}

/// <summary>Record of a sensitive operation (D-037). Append-only; never contains passwords or secrets.</summary>
public class AuditLog : Entity<long>
{
    public DateTime OccurredAtUtc { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string? DetailsJson { get; set; }
}
