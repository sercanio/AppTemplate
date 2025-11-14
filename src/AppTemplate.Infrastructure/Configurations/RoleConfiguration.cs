using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

    builder.Property(r => r.DisplayName)
        .HasConversion(
            displayName => displayName.Value,
            value => new RoleName(value))
        .HasColumnName("display_name")
        .IsRequired();

    builder.Property(r => r.IsDefault)
        .HasColumnName("is_default")
        .IsRequired();

    builder.HasData(
        new
        {
          Id = Role.Admin.Id,
          Name = new RoleName(Role.Admin.Name.Value),
          DisplayName = new RoleName(Role.Admin.DisplayName.Value),
          IsDefault = Role.Admin.IsDefault,
          CreatedBy = "System",
          CreatedOnUtc = DateTime.UtcNow,
        },
        new
        {
          Id = Role.DefaultRole.Id,
          Name = new RoleName(Role.DefaultRole.Name.Value),
          DisplayName = new RoleName(Role.DefaultRole.DisplayName.Value),
          IsDefault = Role.DefaultRole.IsDefault,
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
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.UsersAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.RolesAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.StatisticsRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.UserFollowsRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.UserFollowsCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.UserFollowsUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.UserFollowsDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntriesAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntriesRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntriesCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntriesUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntriesDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitlesAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitlesRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitlesCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitlesUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitlesDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitleFollowsRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitleFollowsCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitleFollowsUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.TitleFollowsDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryLikesRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryLikesCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryLikesUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryLikesDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryBookmarksRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryBookmarksCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryBookmarksUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryBookmarksDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryReportsAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryReportsRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryReportsCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryReportsUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.EntryReportsDelete.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturedEntriesAdmin.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturedEntriesRead.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturedEntriesCreate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturedEntriesUpdate.Id },
                    new { RoleId = Role.Admin.Id, PermissionId = Permission.FeaturedEntriesDelete.Id },

                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.NotificationsRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.NotificationsUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.UsersRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntriesCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntriesRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntriesUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntriesDelete.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitlesCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitlesRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitlesUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.UserFollowsCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.UserFollowsRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.UserFollowsUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitleFollowsCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitleFollowsRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.TitleFollowsUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryLikesCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryLikesRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryLikesUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryLikesDelete.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryBookmarksCreate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryBookmarksRead.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryBookmarksUpdate.Id },
                    new { RoleId = Role.DefaultRole.Id, PermissionId = Permission.EntryBookmarksDelete.Id }
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
