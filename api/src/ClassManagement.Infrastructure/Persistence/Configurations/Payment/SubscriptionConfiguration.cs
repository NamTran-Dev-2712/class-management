using PlanEntity = ClassManagement.Domain.Modules.Payment.Entities.Plan;
using SubscriptionEntity = ClassManagement.Domain.Modules.Payment.Entities.Subscription;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// subscriptions — at most one Active per teacher (partial unique index). teacher_id/plan_id RESTRICT.
// Mutable; updated_at via the set_updated_at trigger (lifecycle sweep updates rows set-based).
public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.ToTable(
            "subscriptions",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_subscriptions_status",
                    "status IN ('Active', 'PastDue', 'Cancelled', 'Expired')"
                );
                t.HasCheckConstraint(
                    "chk_subscriptions_payment_type",
                    "payment_type IN ('Auto', 'Manual')"
                );
                t.HasCheckConstraint(
                    "chk_subscriptions_admin_note_length",
                    "admin_note IS NULL OR length(admin_note) <= 500"
                );
            }
        );

        builder.HasKey(s => s.Id);

        builder
            .Property(s => s.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(s => s.PublicId).HasDatabaseName("uq_subscriptions_public_id").IsUnique();

        builder
            .Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Modules.Payment.Enums.SubscriptionStatus.Active);
        builder
            .Property(s => s.PaymentType)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(Domain.Modules.Payment.Enums.SubscriptionPaymentType.Auto);

        builder.Property(s => s.StartedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<PlanEntity>()
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // BR-8-01: only one Active subscription per teacher.
        builder
            .HasIndex(s => s.TeacherId)
            .HasDatabaseName("uq_subscriptions_teacher_active")
            .IsUnique()
            .HasFilter("status = 'Active'");

        builder
            .HasIndex(s => new { s.TeacherId, s.Status })
            .HasDatabaseName("idx_subscriptions_teacher");
        builder
            .HasIndex(s => s.ExpiresAt)
            .HasDatabaseName("idx_subscriptions_expires")
            .HasFilter("status = 'Active'");
        builder
            .HasIndex(s => s.GracePeriodEndsAt)
            .HasDatabaseName("idx_subscriptions_past_due")
            .HasFilter("status = 'PastDue'");
    }
}
