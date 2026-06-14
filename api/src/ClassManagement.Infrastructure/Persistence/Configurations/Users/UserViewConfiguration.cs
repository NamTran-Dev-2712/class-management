namespace ClassManagement.Infrastructure.Persistence.Configurations.Users;

/// <summary>
/// Maps the read-only <see cref="User"/> projection to the <c>vw_admin_users</c> view.
/// The view itself is created via the <c>add_admin_users_view</c> migration (raw SQL); EF only
/// needs to know its shape. No table is generated for this entity (<c>ToView</c>), and it is not
/// <c>ISoftDeletable</c> — the view already filters out soft-deleted users.
/// </summary>
public sealed class UserViewConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToView("vw_admin_users");
        builder.HasKey(u => u.Id);

        // Postgres text[] → string[]; the view aggregates role names with array_agg.
        builder.Property(u => u.Roles).HasColumnType("text[]");
    }
}
