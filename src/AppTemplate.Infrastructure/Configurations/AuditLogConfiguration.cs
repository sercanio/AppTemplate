using System.Text.Json;
using AppTemplate.Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId);
        builder.Property(a => a.User).IsRequired();
        builder.Property(a => a.Action).IsRequired();
        builder.Property(a => a.Entity).IsRequired();
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.Timestamp);
        builder.Property(a => a.Details);

        // Use JSONB column for additional data  
        builder.Property(a => a.AdditionalData)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                   v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
               );

        // Add optimized indexes for common queries  
        builder.HasIndex(a => a.EntityId).HasDatabaseName("IX_AuditLogs_EntityId");
        builder.HasIndex(a => a.UserId).HasDatabaseName("IX_AuditLogs_UserId");
        builder.HasIndex(a => a.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(a => new { a.Entity, a.Action }).HasDatabaseName("IX_AuditLogs_Entity_Action");

        builder.HasIndex(a => a.AdditionalData)
               .HasDatabaseName("IX_AuditLogs_AdditionalData")
               .HasMethod("gin"); // GIN index is optimized for JSONB  

        builder.HasOne(a => a.AppUser)
                .WithMany(u => u.AuditLogs)  // Change to WithMany
                .HasForeignKey(a => a.AppUserId)
                .IsRequired(false);
    }
}
