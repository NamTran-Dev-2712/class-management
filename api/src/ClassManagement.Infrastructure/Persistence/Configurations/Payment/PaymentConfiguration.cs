using ClassManagement.Infrastructure.Persistence.Configurations.Admin;
using PaymentEntity = ClassManagement.Domain.Modules.Payment.Entities.Payment;
using PlanEntity = ClassManagement.Domain.Modules.Payment.Entities.Plan;
using SubscriptionEntity = ClassManagement.Domain.Modules.Payment.Entities.Subscription;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// payments — provider orders. idempotency_key UNIQUE dedups webhook retries. No updated_at (status
// mutates with completed_at). subscription_id SET NULL; teacher_id/plan_id RESTRICT.
public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable(
            "payments",
            t =>
            {
                t.HasCheckConstraint("chk_payments_provider", "provider IN ('Momo', 'VnPay')");
                t.HasCheckConstraint(
                    "chk_payments_status",
                    "status IN ('Pending', 'Completed', 'Failed', 'Expired')"
                );
                t.HasCheckConstraint(
                    "chk_payments_billing_cycle",
                    "billing_cycle IN ('Monthly', 'Annual')"
                );
                t.HasCheckConstraint("chk_payments_amount", "amount_vnd > 0");
            }
        );

        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(p => p.PublicId).HasDatabaseName("uq_payments_public_id").IsUnique();

        builder.Property(p => p.Provider).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.BillingCycle).HasConversion<string>().HasMaxLength(10);
        builder
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Modules.Payment.Enums.PaymentStatus.Pending);

        builder.Property(p => p.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ProviderOrderId).HasMaxLength(100);
        builder.Property(p => p.ProviderTransactionId).HasMaxLength(100);

        builder
            .Property(p => p.ProviderMetadata)
            .HasColumnType("jsonb")
            .HasConversion(JsonDictionaryConverters.Nullable)
            .Metadata.SetValueComparer(JsonDictionaryConverters.NullableComparer);

        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");

        builder
            .HasOne<SubscriptionEntity>()
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<PlanEntity>()
            .WithMany()
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // CRITICAL: dedup webhook — exactly-once application of a provider order.
        builder
            .HasIndex(p => p.IdempotencyKey)
            .HasDatabaseName("uq_payments_idempotency_key")
            .IsUnique();

        builder
            .HasIndex(p => new { p.TeacherId, p.CreatedAt })
            .HasDatabaseName("idx_payments_teacher")
            .IsDescending(false, true);
        builder
            .HasIndex(p => new { p.Status, p.CreatedAt })
            .HasDatabaseName("idx_payments_status")
            .IsDescending(false, true);
        builder
            .HasIndex(p => new { p.Provider, p.ProviderOrderId })
            .HasDatabaseName("idx_payments_provider_order");
        builder
            .HasIndex(p => p.ExpiresAt)
            .HasDatabaseName("idx_payments_pending_expires")
            .HasFilter("status = 'Pending'");
    }
}
