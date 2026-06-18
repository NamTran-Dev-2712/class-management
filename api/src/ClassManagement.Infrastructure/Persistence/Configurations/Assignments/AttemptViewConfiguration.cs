using ClassManagement.Domain.Modules.Assignments.Entities;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Assignments;

// Maps the read-only AttemptView to vw_attempts (created via raw SQL in the migration). Joins the
// student display name + assignment title + snapshot total point for roster/history screens.
public sealed class AttemptViewConfiguration : IEntityTypeConfiguration<AttemptView>
{
    public void Configure(EntityTypeBuilder<AttemptView> builder)
    {
        builder.ToView("vw_attempts");
        builder.HasKey(a => a.Id);
    }
}
