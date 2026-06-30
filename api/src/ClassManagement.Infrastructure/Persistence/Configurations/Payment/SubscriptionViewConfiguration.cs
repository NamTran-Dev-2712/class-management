using ClassManagement.Domain.Modules.Payment.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// Maps the read-only SubscriptionView to vw_subscriptions (created via raw SQL in the migration).
public sealed class SubscriptionViewConfiguration : IEntityTypeConfiguration<SubscriptionView>
{
    public void Configure(EntityTypeBuilder<SubscriptionView> builder)
    {
        builder.ToView("vw_subscriptions");
        builder.HasKey(s => s.Id);
    }
}
