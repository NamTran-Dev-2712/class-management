using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_admin_users_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Read model for Admin user management: one row per non-deleted user with their role
            // names aggregated into a text[]. Lets the list query reuse BaseGetQueryHandler without
            // leaking the Identity ApplicationUser type into Application/Domain.
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_admin_users AS
SELECT
    u.id,
    u.public_id,
    u.display_name,
    u.email,
    u.email_confirmed,
    u.phone_number,
    u.is_active,
    u.is_locked,
    u.locked_at,
    u.last_login_at,
    u.created_at,
    u.updated_at,
    COALESCE(array_agg(r.name) FILTER (WHERE r.name IS NOT NULL), '{}') AS roles
FROM users u
LEFT JOIN user_roles ur ON ur.user_id = u.id
LEFT JOIN roles r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
GROUP BY u.id;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_admin_users;");
        }
    }
}
