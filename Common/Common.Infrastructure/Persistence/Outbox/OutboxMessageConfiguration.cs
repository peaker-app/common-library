using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(500).IsRequired();
        builder.Property(message => message.Content).HasColumnName("content").IsRequired();
        builder.Property(message => message.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(message => message.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(message => message.Error).HasColumnName("error");
        builder.Property(message => message.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
        builder.Property(message => message.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");

        builder
            .HasIndex(message => new { message.ProcessedAtUtc, message.NextAttemptAtUtc })
            .HasDatabaseName("ix_outbox_messages_processed_at_utc");
    }
}
