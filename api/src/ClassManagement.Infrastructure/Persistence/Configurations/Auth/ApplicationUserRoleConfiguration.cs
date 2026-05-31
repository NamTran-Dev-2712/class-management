using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Infrastructure.Persistence.Configurations.Auth;

public sealed class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.AssignedAt).HasDefaultValueSql("now()");

        // FK: admin who assigned the role — SET NULL on delete
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(ur => ur.AssignedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Index: find all users with a given role
        builder.HasIndex(ur => ur.RoleId).HasDatabaseName("idx_user_roles_role_id");

        // Identity already creates FK for UserId+RoleId via IdentityDbContext base
    }
}
