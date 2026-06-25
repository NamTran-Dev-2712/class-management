using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_reports_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // vw_reports — a report joined to its reporter and reviewing admin for display.
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_reports AS
                SELECT
                    r.id,
                    r.public_id,
                    r.reporter_id,
                    reporter.public_id    AS reporter_public_id,
                    reporter.display_name AS reporter_name,
                    reporter.email        AS reporter_email,
                    r.target_type,
                    r.target_public_id,
                    r.reason,
                    r.description,
                    r.status,
                    r.admin_id,
                    admin_u.display_name  AS admin_name,
                    r.admin_action,
                    r.admin_note,
                    r.resolved_at,
                    r.created_at,
                    r.updated_at
                FROM reports r
                JOIN users reporter ON reporter.id = r.reporter_id
                LEFT JOIN users admin_u ON admin_u.id = r.admin_id;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_reports;");
        }
    }
}
