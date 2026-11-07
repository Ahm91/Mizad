using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Administration;
using Mizad.Domain.Identity;

namespace Mizad.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", t => t.HasCheckConstraint("CK_Users_FailedLoginCount", "[FailedLoginCount] >= 0"));
        builder.Property(x => x.UserName).HasMaxLength(50);
        builder.HasIndex(x => x.UserName).IsUnique().HasDatabaseName("UX_Users_UserName");
        builder.Property(x => x.FullName).HasMaxLength(150);
        builder.Property(x => x.PasswordHash).AsCode(200);
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Roles_Name");
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.HasMany(x => x.Permissions).WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class RolePermissionGrantConfiguration : IEntityTypeConfiguration<RolePermissionGrant>
{
    public void Configure(EntityTypeBuilder<RolePermissionGrant> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionCode });
        builder.Property(x => x.PermissionCode).AsCode(100);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(x => x.UserName).HasMaxLength(50);
        builder.Property(x => x.MachineName).HasMaxLength(100);
        builder.Property(x => x.Action).AsCode(100);
        builder.Property(x => x.EntityType).AsCode(100);
        builder.Property(x => x.EntityId).AsCode(50);
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.DetailsJson);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OccurredAtUtc).IsDescending();
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
    }
}

internal sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).AsCode(100);
        builder.Property(x => x.Value);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences", t => t.HasCheckConstraint("CK_DocumentSequences_NextNumber", "[NextNumber] >= 1"));
        builder.HasKey(x => x.DocumentType);
        builder.Property(x => x.DocumentType).AsCode(30);
        builder.Property(x => x.Prefix).HasMaxLength(10);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class BackupRecordConfiguration : IEntityTypeConfiguration<BackupRecord>
{
    public void Configure(EntityTypeBuilder<BackupRecord> builder)
    {
        builder.ToTable("BackupHistory");
        builder.Property(x => x.FilePath).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.MachineName).HasMaxLength(100);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.StartedAtUtc).IsDescending();
    }
}
