using InvoiceEntity = ClassManagement.Domain.Modules.Payment.Entities.Invoice;
using PaymentEntity = ClassManagement.Domain.Modules.Payment.Entities.Payment;
using SubscriptionEntity = ClassManagement.Domain.Modules.Payment.Entities.Subscription;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// invoices — immutable, one-to-one with a payment. CreatedAt maps to issued_at. No updated_at, no soft
// delete. payment_id RESTRICT + UNIQUE; subscription_id SET NULL; teacher_id RESTRICT.
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable(
            "invoices",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_invoices_billing_cycle",
                    "billing_cycle IN ('Monthly', 'Annual')"
                );
                t.HasCheckConstraint("chk_invoices_amount", "amount_vnd > 0");
            }
        );

        builder.HasKey(i => i.Id);

        builder
            .Property(i => i.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(i => i.PublicId).HasDatabaseName("uq_invoices_public_id").IsUnique();

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.Property(i => i.PlanName).IsRequired().HasMaxLength(50);
        builder.Property(i => i.BillingCycle).HasConversion<string>().HasMaxLength(10);
        builder.Property(i => i.PdfUrl).HasMaxLength(500);

        // Append-only: BaseEntity.CreatedAt is the issue timestamp.
        builder.Property(i => i.CreatedAt).HasColumnName("issued_at").HasDefaultValueSql("now()");

        builder
            .HasOne<PaymentEntity>()
            .WithMany()
            .HasForeignKey(i => i.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<SubscriptionEntity>()
            .WithMany()
            .HasForeignKey(i => i.SubscriptionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(i => i.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PaymentId).HasDatabaseName("uq_invoices_payment").IsUnique();
        builder.HasIndex(i => i.InvoiceNumber).HasDatabaseName("uq_invoices_number").IsUnique();
        builder
            .HasIndex(i => new { i.TeacherId, i.CreatedAt })
            .HasDatabaseName("idx_invoices_teacher")
            .IsDescending(false, true);
    }
}
