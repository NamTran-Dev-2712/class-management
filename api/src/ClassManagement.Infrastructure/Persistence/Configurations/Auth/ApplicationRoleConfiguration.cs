namespace ClassManagement.Infrastructure.Persistence.Configurations.Auth;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable(
            "roles",
            t => t.HasCheckConstraint("chk_roles_name", "name IN ('Student', 'Teacher', 'Admin')")
        );

        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
    }
}
