using EcoFind.Domain.AppUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFind.Infrastructure.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        // Column Mapping for IdentityId
        builder.Property(u => u.IdentityId)
               .HasColumnName("identity_id")
               .IsRequired();

        // Unique index on IdentityId
        builder.HasIndex(u => u.IdentityId).IsUnique();

        // Configure the relationship with IdentityUser.
        // Assumes IdentityUser is defined in your Identity context and here
        // we only have a navigation from AppUser to IdentityUser.
        builder.HasOne(u => u.IdentityUser)
               .WithOne() // No inverse navigation on IdentityUser.
               .HasForeignKey<AppUser>(u => u.IdentityId)
               .IsRequired();

        // Owned type for NotificationPreference
        builder.OwnsOne(u => u.NotificationPreference, np =>
        {
            np.Property(p => p.IsInAppNotificationEnabled)
              .HasColumnName("in_app_notification");
            np.Property(p => p.IsEmailNotificationEnabled)
              .HasColumnName("email_notification");
            np.Property(p => p.IsPushNotificationEnabled)
              .HasColumnName("push_notification");
        });

        // Seeding an admin user.
        // Note: Adjust the CreateWithoutRolesForSeeding method if additional value objects are needed.
        var adminUser = AppUser.CreateWithoutRolesForSeeding();
        adminUser.SetIdentityId("b3398ff2-1b43-4af7-812d-eb4347eecbb8");

        // Basic entity seeding
        builder.HasData(new
        {
            adminUser.Id,
            IdentityId = adminUser.IdentityId,
            CreatedBy = adminUser.CreatedBy,
            CreatedOnUtc = adminUser.CreatedOnUtc,
            UpdatedBy = adminUser.UpdatedBy,
            UpdatedOnUtc = adminUser.UpdatedOnUtc
        });

        // Owned type seeding for NotificationPreference
        builder.OwnsOne(u => u.NotificationPreference).HasData(new
        {
            AppUserId = adminUser.Id,
            IsInAppNotificationEnabled = adminUser.NotificationPreference.IsInAppNotificationEnabled,
            IsEmailNotificationEnabled = adminUser.NotificationPreference.IsEmailNotificationEnabled,
            IsPushNotificationEnabled = adminUser.NotificationPreference.IsPushNotificationEnabled
        });

    }
}
