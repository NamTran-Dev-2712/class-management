using PlanEntity = ClassManagement.Domain.Modules.Payment.Entities.Plan;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// plans — seeded service plans (Free/Pro). Mutable price/limits; updated_at via the set_updated_at
// trigger. NULL limit = unlimited. Unique on (name, billing_cycle) so Pro can have monthly + annual rows.
public sealed class PlanConfiguration : IEntityTypeConfiguration<PlanEntity>
{
    public void Configure(EntityTypeBuilder<PlanEntity> builder)
    {
        builder.ToTable(
            "plans",
            t =>
            {
                t.HasCheckConstraint(
                    "chk_plans_name_length",
                    "length(name) >= 2 AND length(name) <= 50"
                );
                t.HasCheckConstraint(
                    "chk_plans_billing_cycle",
                    "billing_cycle IS NULL OR billing_cycle IN ('Monthly', 'Annual')"
                );
                t.HasCheckConstraint("chk_plans_price", "price_vnd >= 0");
                t.HasCheckConstraint(
                    "chk_plans_max_classes",
                    "max_classes IS NULL OR max_classes > 0"
                );
                t.HasCheckConstraint(
                    "chk_plans_max_questions",
                    "max_questions IS NULL OR max_questions > 0"
                );
                t.HasCheckConstraint("chk_plans_max_exams", "max_exams IS NULL OR max_exams > 0");
                t.HasCheckConstraint(
                    "chk_plans_max_students",
                    "max_students_per_class IS NULL OR max_students_per_class > 0"
                );
            }
        );

        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();
        builder.HasIndex(p => p.PublicId).HasDatabaseName("uq_plans_public_id").IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
        builder.Property(p => p.BillingCycle).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.PriceVnd).IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.DisplayOrder).HasDefaultValue(0);

        builder
            .Property(p => p.Features)
            .HasColumnType("jsonb")
            .HasConversion(PaymentJsonConverters.NullableStringList)
            .Metadata.SetValueComparer(PaymentJsonConverters.NullableStringListComparer);

        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        // One row per (name, billing_cycle): e.g. Pro/Monthly and Pro/Annual coexist; Free has NULL cycle.
        builder
            .HasIndex(p => new { p.Name, p.BillingCycle })
            .HasDatabaseName("uq_plans_name_cycle")
            .IsUnique();
        builder
            .HasIndex(p => new { p.IsActive, p.DisplayOrder })
            .HasDatabaseName("idx_plans_active_order");
    }
}
