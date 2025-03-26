using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Feature).HasColumnName("feature").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();

        builder.HasData(
            Permission.UsersRead,
            Permission.UsersCreate,
            Permission.UsersUpdate,
            Permission.UsersDelete,

            Permission.RolesRead,
            Permission.RolesCreate,
            Permission.RolesUpdate,
            Permission.RolesDelete,

            Permission.PermissionsRead,
            Permission.AuditLogsRead,

            Permission.NotificationsRead,
            Permission.NotificationsUpdate
        );
    }
}
