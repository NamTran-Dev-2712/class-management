using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// notifications — append-only aside from status/read_at. user_id CASCADE (ephemeral). 24h idempotency
// index on (user_id, event_type, payload->>'reference_id') built via raw SQL in the migration.
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable(
            "notifications",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_notifications_status",
                    "status IN ('Unread', 'Read', 'Archived')"
                );
                t.HasCheckConstraint(
                    "chk_notifications_title_length",
                    "length(title) >= 1 AND length(title) <= 200"
                );
                t.HasCheckConstraint(
                    "chk_notifications_body_length",
                    "body IS NULL OR length(body) <= 500"
                );
            }
        );

        builder.HasKey(n => n.Id);

        builder
            .Property(n => n.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(n => n.PublicId).HasDatabaseName("uq_notifications_public_id").IsUnique();

        builder.Property(n => n.EventType).HasConversion<string>().HasMaxLength(40);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).HasMaxLength(500);
        builder.Property(n => n.Link).HasMaxLength(500);
        builder.Property(n => n.ReferenceId).HasMaxLength(100);
        builder
            .Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Modules.Admin.Enums.NotificationStatus.Unread);
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("now()");

        builder
            .Property(n => n.Payload)
            .HasColumnType("jsonb")
            .HasConversion(JsonDictionaryConverters.Nullable)
            .Metadata.SetValueComparer(JsonDictionaryConverters.NullableComparer);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // HOT: a user's notification list, newest first.
        builder
            .HasIndex(n => new
            {
                n.UserId,
                n.Status,
                n.CreatedAt,
            })
            .HasDatabaseName("idx_notifications_user_status")
            .IsDescending(false, false, true);
        // Unread badge count.
        builder
            .HasIndex(n => n.UserId)
            .HasDatabaseName("idx_notifications_user_unread")
            .HasFilter("status = 'Unread'");
        builder
            .HasIndex(n => new { n.EventType, n.CreatedAt })
            .HasDatabaseName("idx_notifications_event_type")
            .IsDescending(false, true);
        // Idempotent background sends: at most one notification per (user, event, reference). NOW() is
        // not IMMUTABLE so the doc's 24h window can't live in the predicate — the job dedups by time,
        // this index is the hard backstop for events that carry a reference id.
        builder
            .HasIndex(n => new
            {
                n.UserId,
                n.EventType,
                n.ReferenceId,
            })
            .HasDatabaseName("uq_notifications_idempotent")
            .IsUnique()
            .HasFilter("reference_id IS NOT NULL");
    }
}
