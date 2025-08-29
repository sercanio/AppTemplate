using AppTemplate.Domain.AppUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
  public void Configure(EntityTypeBuilder<AppUser> builder)
  {
    builder.ToTable("AppUsers");
    builder.HasKey(u => u.Id);

    // Identity link
    builder.Property(u => u.IdentityId)
           .HasColumnName("identity_id")
           .IsRequired();

    builder.HasIndex(u => u.IdentityId).IsUnique();

    builder.HasOne(u => u.IdentityUser)
           .WithOne()
           .HasForeignKey<AppUser>(u => u.IdentityId)
           .IsRequired();

    // your existing owned NotificationPreference
    builder.OwnsOne(u => u.NotificationPreference, np =>
    {
      np.Property(p => p.IsInAppNotificationEnabled).HasColumnName("in_app_notification");
      np.Property(p => p.IsEmailNotificationEnabled).HasColumnName("email_notification");
      np.Property(p => p.IsPushNotificationEnabled).HasColumnName("push_notification");
    });

    builder.Property(u => u.UserChangedItsUsername)
           .HasColumnName("user_changed_its_username")
           .IsRequired()
           .HasDefaultValue(false);

    builder.Property(u => u.Biography)
           .HasColumnName("biography")
           .HasMaxLength(1000)
           .IsRequired(false);

    // ——— AUDIT (self-refs) ———
    builder.Property(u => u.CreatedById)
           .HasColumnName("created_by_id")
           .IsRequired(false);

    builder.Property(u => u.UpdatedById)
           .HasColumnName("updated_by_id")
           .IsRequired(false);

    builder.Property(u => u.DeletedById)
           .HasColumnName("deleted_by_id")
           .IsRequired(false);

    builder.HasOne(u => u.CreatedBy)
           .WithMany()    // no inverse collection
           .HasForeignKey(u => u.CreatedById)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(u => u.UpdatedBy)
           .WithMany(u => u.UpdatedUsers)
           .HasForeignKey(u => u.UpdatedById)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(u => u.DeletedBy)
           .WithMany(u => u.DeletedUsers)
           .HasForeignKey(u => u.DeletedById)
           .OnDelete(DeleteBehavior.Restrict);

    // ——— (optional) seed data ———
    var admin = AppUser.CreateWithoutRolesForSeeding();
    admin.SetIdentityId("b3398ff2-1b43-4af7-812d-eb4347eecbb8");

    builder.HasData(new
    {
      admin.Id,
      admin.IdentityId,
      CreatedById = admin.CreatedById,
      CreatedOnUtc = admin.CreatedOnUtc,
      UpdatedById = admin.UpdatedById,
      UpdatedOnUtc = admin.UpdatedOnUtc,
      admin.UserChangedItsUsername
    });

    builder.OwnsOne(u => u.NotificationPreference).HasData(new
    {
      AppUserId = admin.Id,
      admin.NotificationPreference.IsInAppNotificationEnabled,
      admin.NotificationPreference.IsEmailNotificationEnabled,
      admin.NotificationPreference.IsPushNotificationEnabled
    });
  }
}
