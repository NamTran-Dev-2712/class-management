using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_audit_logs_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // vw_audit_logs — audit rows joined to the actor user for display. metadata cast to text so
            // it maps to a plain string in the read model. Append-only source; no soft-delete filter.
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_audit_logs AS
                SELECT
                    al.id,
                    al.action,
                    al.actor_id,
                    u.public_id   AS actor_public_id,
                    u.display_name AS actor_name,
                    u.email        AS actor_email,
                    al.actor_role,
                    al.target_type,
                    al.target_public_id,
                    al.metadata::text AS metadata,
                    al.ip_address,
                    al.user_agent,
                    al.created_at
                FROM audit_logs al
                LEFT JOIN users u ON u.id = al.actor_id;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_audit_logs;");
        }
    }
}
