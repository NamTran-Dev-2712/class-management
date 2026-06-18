using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Immutable snapshot container (1-1 with assignment). Append-only: no UpdatedAt, no soft-delete.
public sealed class AssignmentSnapshotConfiguration : IEntityTypeConfiguration<AssignmentSnapshot>
{
    public void Configure(EntityTypeBuilder<AssignmentSnapshot> builder)
    {
        builder.ToTable(
            "assignment_snapshots",
            t =>
            {
                t.HasCheckConstraint("chk_assignment_snapshots_total_point", "total_point > 0");
                t.HasCheckConstraint(
                    "chk_assignment_snapshots_total_questions",
                    "total_questions > 0"
                );
            }
        );

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TotalPoint).HasColumnType("numeric(8,2)");
        builder.Property(s => s.SnapshotCreatedAt).HasDefaultValueSql("now()");

        // 1-1 with assignment.
        builder
            .HasIndex(s => s.AssignmentId)
            .HasDatabaseName("uq_assignment_snapshots_assignment")
            .IsUnique();

        // Owns the snapshot questions; RESTRICT (immutable, never orphaned).
        builder
            .HasMany(s => s.Questions)
            .WithOne()
            .HasForeignKey(q => q.SnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
