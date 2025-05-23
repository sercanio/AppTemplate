﻿using System.Text.Json;
using AppTemplate.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired()
            .HasComment("The ID of the user who owns this notification");

        builder.Property(n => n.UserName)
            .IsRequired()
            .HasMaxLength(256)
            .HasComment("The username of the notification owner");

        builder.Property(n => n.Action)
            .HasMaxLength(100)
            .HasComment("The action that triggered this notification");

        builder.Property(n => n.Entity)
            .HasMaxLength(100)
            .HasComment("The entity type related to this notification");

        builder.Property(n => n.EntityId)
            .HasMaxLength(100)
            .HasComment("The ID of the related entity");

        builder.Property(n => n.Timestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("When the notification was created");

        builder.Property(n => n.Details)
            .HasMaxLength(1000)
            .HasComment("Detailed description of the notification");

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false)
            .HasComment("Whether the notification has been read");

        builder.HasOne(n => n.AppUser)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

        // Define a ValueComparer for AdditionalData
        var dictionaryComparer = new ValueComparer<Dictionary<string, object>?>(
            (d1, d2) => d1 != null && d2 != null && d1.SequenceEqual(d2),
            d => d != null ? d.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
            d => d != null ? d.ToDictionary(entry => entry.Key, entry => entry.Value) : null
        );

        // Configure AdditionalData with a ValueComparer
        builder.Property(n => n.AdditionalData)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, new JsonSerializerOptions
                {
                    WriteIndented = false
                }),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }))

            .HasComment("Additional JSON data associated with the notification")
            .Metadata.SetValueComparer(dictionaryComparer);

        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId")
            .HasFilter("\"DeletedOnUtc\" IS NULL");

        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead")
            .HasFilter("\"DeletedOnUtc\" IS NULL")
            .IncludeProperties(n => new { n.Timestamp, n.Details });

        builder.HasIndex(n => n.Timestamp)
            .HasDatabaseName("IX_Notifications_Timestamp")
            .HasFilter("\"DeletedOnUtc\" IS NULL");

        builder.HasIndex(n => new { n.Entity, n.EntityId })
            .HasDatabaseName("IX_Notifications_Entity_EntityId")
            .HasFilter("\"DeletedOnUtc\" IS NULL")
            .IncludeProperties(n => new { n.UserId, n.Timestamp });

        builder.HasIndex(n => n.AdditionalData)
            .HasDatabaseName("IX_Notifications_AdditionalData")
            .HasMethod("gin")
            .HasFilter("\"DeletedOnUtc\" IS NULL");

        builder.HasIndex(n => new { n.UserId, n.Timestamp })
            .HasDatabaseName("IX_Notifications_Unread")
            .HasFilter("\"IsRead\" = false AND \"DeletedOnUtc\" IS NULL")
            .IncludeProperties(n => new { n.Details, n.Action });

        builder.Property(n => n.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.UpdatedBy)
            .HasMaxLength(256);

        builder.HasQueryFilter(n => n.DeletedOnUtc == null);
    }
}
