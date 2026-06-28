using ClassManagement.Domain.Modules.Payment.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Payment;

// Maps the read-only PaymentView to vw_payments (created via raw SQL in the migration).
public sealed class PaymentViewConfiguration : IEntityTypeConfiguration<PaymentView>
{
    public void Configure(EntityTypeBuilder<PaymentView> builder)
    {
        builder.ToView("vw_payments");
        builder.HasKey(p => p.Id);
    }
}
