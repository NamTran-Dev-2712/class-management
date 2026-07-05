using ClassManagement.Domain.Modules.Assignments.Constants;
using ClassManagement.Domain.Modules.Assignments.Entities;
using ClassManagement.Infrastructure.Persistence.Configurations.Admin;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// attempt_events — append-only proctoring evidence (MVP-10, BR-10-02). No PublicId, no updated_at.
public sealed class AttemptEventConfiguration : IEntityTypeConfiguration<AttemptEvent>
{
    public void Configure(EntityTypeBuilder<AttemptEvent> builder)
    {
        var eventTypes = string.Join(", ", AttemptEventTypes.All.Select(e => $"'{e}'"));

        builder.ToTable(
            "attempt_events",
            t => t.HasCheckConstraint("chk_attempt_events_type", $"event_type IN ({eventTypes})")
        );

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

        builder
            .Property(e => e.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(JsonDictionaryConverters.Nullable)
            .Metadata.SetValueComparer(JsonDictionaryConverters.NullableComparer);

        // Timeline read: all events of one attempt, oldest first.
        builder
            .HasIndex(e => new { e.AttemptId, e.OccurredAt })
            .HasDatabaseName("idx_attempt_events_attempt");
    }
}
