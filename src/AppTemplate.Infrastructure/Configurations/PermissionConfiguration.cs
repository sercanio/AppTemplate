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
      // users
      Permission.UsersAdmin,
      Permission.UsersRead,
      Permission.UsersCreate,
      Permission.UsersUpdate,
      Permission.UsersDelete,

      // roles
      Permission.RolesAdmin,
      Permission.RolesRead,
      Permission.RolesCreate,
      Permission.RolesUpdate,
      Permission.RolesDelete,

      // permissions
      Permission.PermissionsRead,

      // auditlogs
      Permission.AuditLogsRead,

      // notifications
      Permission.NotificationsRead,
      Permission.NotificationsUpdate,

      // statistics
      Permission.StatisticsRead,

      // userfollows
      Permission.UserFollowsRead,
      Permission.UserFollowsCreate,
      Permission.UserFollowsUpdate,
      Permission.UserFollowsDelete,

      // entries
      Permission.EntriesAdmin,
      Permission.EntriesRead,
      Permission.EntriesCreate,
      Permission.EntriesUpdate,
      Permission.EntriesDelete,

      // titles
      Permission.TitlesAdmin,
      Permission.TitlesRead,
      Permission.TitlesCreate,
      Permission.TitlesUpdate,
      Permission.TitlesDelete,

      // titlefollows
      Permission.TitleFollowsRead,
      Permission.TitleFollowsCreate,
      Permission.TitleFollowsUpdate,
      Permission.TitleFollowsDelete,

      // entrylikes
      Permission.EntryLikesRead,
      Permission.EntryLikesCreate,
      Permission.EntryLikesUpdate,
      Permission.EntryLikesDelete,

      // entrybookmarks
      Permission.EntryBookmarksRead,
      Permission.EntryBookmarksCreate,
      Permission.EntryBookmarksUpdate,
      Permission.EntryBookmarksDelete,

      // entryreports
      Permission.EntryReportsAdmin,
      Permission.EntryReportsRead,
      Permission.EntryReportsCreate,
      Permission.EntryReportsUpdate,
      Permission.EntryReportsDelete,

      // featuredentries
      Permission.FeaturedEntriesAdmin,
      Permission.FeaturedEntriesRead,
      Permission.FeaturedEntriesCreate,
      Permission.FeaturedEntriesUpdate,
      Permission.FeaturedEntriesDelete);
  }
}