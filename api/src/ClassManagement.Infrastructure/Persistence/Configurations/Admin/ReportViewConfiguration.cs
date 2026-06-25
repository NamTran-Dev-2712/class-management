using ClassManagement.Domain.Modules.Admin.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Admin;

// Maps the read-only ReportView to vw_reports (created via raw SQL in the migration).
public sealed class ReportViewConfiguration : IEntityTypeConfiguration<ReportView>
{
    public void Configure(EntityTypeBuilder<ReportView> builder)
    {
        builder.ToView("vw_reports");
        builder.HasKey(r => r.Id);
    }
}
