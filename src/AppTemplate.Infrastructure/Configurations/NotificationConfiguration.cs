using AppTemplate.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
  public void Configure(EntityTypeBuilder<Notification> builder)
  {
    builder.ToTable("Notifications");

    builder.HasKey(n => n.Id);

    builder.Property(n => n.RecipientId)
        .IsRequired()
        .HasComment("The ID of the user who owns this notification");

    // Navigation property configuration
    builder.HasOne(n => n.Recipient)
        .WithMany(u => u.Notifications)
        .HasForeignKey(n => n.RecipientId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Property(n => n.Title)
        .IsRequired()
        .HasMaxLength(256)
        .HasComment("Notification title");

    builder.Property(n => n.Message)
        .IsRequired()
        .HasMaxLength(1000)
        .HasComment("Notification message");

    builder.Property(n => n.Type)
        .IsRequired()
        .HasComment("Notification type");

    builder.Property(n => n.CreatedOnUtc)
        .IsRequired()
        .HasComment("When the notification was created");

    builder.Property(n => n.IsRead)
        .HasDefaultValue(false)
        .HasComment("Whether the notification has been read");

    builder.HasQueryFilter(n => n.DeletedOnUtc == null);

    // Indexes for efficient querying
    builder.HasIndex(n => n.RecipientId)
        .HasDatabaseName("IX_Notifications_RecipientId")
        .HasFilter("\"DeletedOnUtc\" IS NULL");

    builder.HasIndex(n => new { n.RecipientId, n.IsRead })
        .HasDatabaseName("IX_Notifications_RecipientId_IsRead")
        .HasFilter("\"DeletedOnUtc\" IS NULL")
        .IncludeProperties(n => new { n.CreatedOnUtc, n.Title });

    builder.HasIndex(n => n.CreatedOnUtc)
        .HasDatabaseName("IX_Notifications_CreatedOnUtc")
        .HasFilter("\"DeletedOnUtc\" IS NULL");
  }
}