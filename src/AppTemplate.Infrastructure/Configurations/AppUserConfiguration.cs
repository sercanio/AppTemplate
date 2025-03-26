using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.HasKey(user => user.Id);

        builder.Property(u => u.IdentityId)
               .HasColumnName("identity_id")
               .IsRequired();

        builder.HasIndex(u => u.IdentityId).IsUnique();

        builder.HasOne(u => u.IdentityUser)
               .WithOne()
               .HasForeignKey<AppUser>(u => u.IdentityId)
               .IsRequired();

        builder.OwnsOne(u => u.NotificationPreference, np =>
        {
            np.Property(p => p.IsInAppNotificationEnabled)
              .HasColumnName("in_app_notification");
            np.Property(p => p.IsEmailNotificationEnabled)
              .HasColumnName("email_notification");
            np.Property(p => p.IsPushNotificationEnabled)
              .HasColumnName("push_notification");
        });

        var adminUser = AppUser.CreateWithoutRolesForSeeding();
        adminUser.SetIdentityId("b3398ff2-1b43-4af7-812d-eb4347eecbb8");

        builder.HasData(new
        {
            adminUser.Id,
            IdentityId = adminUser.IdentityId,
            CreatedBy = adminUser.CreatedBy,
            CreatedOnUtc = adminUser.CreatedOnUtc,
            UpdatedBy = adminUser.UpdatedBy,
            UpdatedOnUtc = adminUser.UpdatedOnUtc
        });

        builder.OwnsOne(u => u.NotificationPreference).HasData(new
        {
            AppUserId = adminUser.Id,
            IsInAppNotificationEnabled = adminUser.NotificationPreference.IsInAppNotificationEnabled,
            IsEmailNotificationEnabled = adminUser.NotificationPreference.IsEmailNotificationEnabled,
            IsPushNotificationEnabled = adminUser.NotificationPreference.IsPushNotificationEnabled
        });

    }
}
