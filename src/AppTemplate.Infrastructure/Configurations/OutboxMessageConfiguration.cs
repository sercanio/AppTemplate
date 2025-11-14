using AppTemplate.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTemplate.Infrastructure.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    builder.ToTable("outbox_messages");

    builder.HasKey(outboxMessage => outboxMessage.Id);

    builder.Property(outboxMessage => outboxMessage.Id)
           .IsRequired()
           .HasComment("Unique identifier for the outbox message");

    builder.Property(outboxMessage => outboxMessage.OccurredOnUtc)
           .IsRequired()
           .HasComment("When the domain event occurred");

    builder.Property(outboxMessage => outboxMessage.Type)
           .IsRequired()
           .HasMaxLength(255)
           .HasComment("Type of the domain event");

    builder.Property(outboxMessage => outboxMessage.Content)
           .HasColumnType("jsonb")
           .IsRequired()
           .HasComment("Serialized domain event content");

    builder.Property(outboxMessage => outboxMessage.ProcessedOnUtc)
           .IsRequired(false)
           .HasComment("When the message was processed (null if not processed)");

    builder.Property(outboxMessage => outboxMessage.Error)
           .IsRequired(false)
           .HasComment("Error details if processing failed");

    // Indexes for efficient querying
    builder.HasIndex(outboxMessage => outboxMessage.ProcessedOnUtc)
           .HasDatabaseName("IX_OutboxMessages_ProcessedOnUtc")
           .HasFilter("\"ProcessedOnUtc\" IS NULL");

    builder.HasIndex(outboxMessage => outboxMessage.OccurredOnUtc)
           .HasDatabaseName("IX_OutboxMessages_OccurredOnUtc");
  }
}
