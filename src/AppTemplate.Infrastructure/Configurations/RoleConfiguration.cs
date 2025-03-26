using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasConversion(
                roleName => roleName.Value,
                value => new RoleName(value))
            .HasColumnName("name")
            .IsRequired();

        builder.Property(r => r.IsDefault)
            .HasConversion(
                flag => flag.Value,
                value => new RoleDefaultFlag(value))
            .HasColumnName("is_default")
            .IsRequired();

        builder.HasData(
            new
            {
                Id = Role.Admin.Id,
                Name = new RoleName(Role.Admin.Name.Value),
                IsDefault = new RoleDefaultFlag(Role.Admin.IsDefault.Value),
                CreatedBy = "System",
                CreatedOnUtc = DateTime.UtcNow,
            },
            new
            {
                Id = Role.DefaultRole.Id,
                Name = new RoleName(Role.DefaultRole.Name.Value),
                IsDefault = new RoleDefaultFlag(Role.DefaultRole.IsDefault.Value),
                CreatedBy = "System",
                CreatedOnUtc = DateTime.UtcNow,
            }
        );

        builder
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermission",
                    rp => rp
                        .HasOne<Permission>()
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK_RolePermission_Permissions_PermissionId"),
                    rp => rp
                        .HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_RolePermission_Roles_RoleId"),
                    rp =>
                    {
                        rp.HasKey("RoleId", "PermissionId");

                        rp.HasData(
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersRead.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersCreate.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersUpdate.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersDelete.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.RolesRead.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.RolesCreate.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.RolesUpdate.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.RolesDelete.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.PermissionsRead.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.AuditLogsRead.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.NotificationsRead.Id },
                        new { RoleId = Role.Admin.Id, PermissionId = Permission.NotificationsUpdate.Id },
                        new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.NotificationsRead.Id },
                        new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.NotificationsUpdate.Id },
                        new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.UsersRead.Id }
                    );
                    }
            );

        builder
            .HasMany(r => r.Users)
            .WithMany(u => u.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RoleUser",
                ru => ru
                    .HasOne<AppUser>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .HasConstraintName("FK_RoleUser_AppUsers_UserId"),
                ru => ru
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .HasConstraintName("FK_RoleUser_Roles_RoleId"),
                ru =>
                {
                    ru.HasKey("RoleId", "UserId");

                    ru.HasData(
                        new
                        {
                            RoleId = Role.Admin.Id,
                            UserId = Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7")
                        },
                        new
                        {
                            RoleId = Role.DefaultRole.Id,
                            UserId = Guid.Parse("55c7f429-0916-4d84-8b76-d45185d89aa7")
                        }
                    );
                }
            );
    }
}
