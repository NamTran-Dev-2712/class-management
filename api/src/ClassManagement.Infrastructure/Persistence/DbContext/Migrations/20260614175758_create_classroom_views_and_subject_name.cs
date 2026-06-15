using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_classroom_views_and_subject_name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "subject_name",
                table: "classes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_classes_subject_name_length",
                table: "classes",
                sql: "subject_name IS NULL OR length(subject_name) <= 200"
            );

            // Read model for class lists/detail: one row per non-deleted class + owner display
            // name/email + approved/pending member counts. Lets list queries reuse
            // BaseGetQueryHandler without joining the Identity ApplicationUser type.
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_classes AS
SELECT
    c.id,
    c.public_id,
    c.name,
    c.description,
    c.subject_id,
    sub.public_id  AS subject_public_id,
    c.subject_name,
    c.invite_code,
    c.status,
    c.cover_image_url,
    c.created_at,
    c.updated_at,
    c.owner_id,
    o.public_id    AS owner_public_id,
    o.display_name AS owner_name,
    o.email        AS owner_email,
    COALESCE(mc.approved_count, 0) AS approved_member_count,
    COALESCE(mc.pending_count, 0)  AS pending_count
FROM classes c
JOIN users o ON o.id = c.owner_id
LEFT JOIN subjects sub ON sub.id = c.subject_id
LEFT JOIN (
    SELECT class_id,
           COUNT(*) FILTER (WHERE status = 'Approved') AS approved_count,
           COUNT(*) FILTER (WHERE status = 'Pending')  AS pending_count
    FROM class_memberships
    GROUP BY class_id
) mc ON mc.class_id = c.id
WHERE c.deleted_at IS NULL;
"
            );

            // Read model for membership/roster/'my classes' lists: one row per membership joined to
            // its student, class, and class owner.
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_class_members AS
SELECT
    m.id,
    m.public_id,
    m.status,
    m.joined_at,
    m.processed_at,
    m.rejection_reason,
    m.created_at,
    m.class_id,
    c.public_id    AS class_public_id,
    c.name         AS class_name,
    c.subject_name,
    c.status       AS class_status,
    c.owner_id,
    o.display_name AS owner_name,
    m.student_id,
    s.public_id    AS student_public_id,
    s.display_name AS student_name,
    s.email        AS student_email
FROM class_memberships m
JOIN classes c ON c.id = m.class_id
JOIN users s ON s.id = m.student_id
JOIN users o ON o.id = c.owner_id
WHERE c.deleted_at IS NULL;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop views first — they depend on the subject_name column.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_class_members;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_classes;");

            migrationBuilder.DropCheckConstraint(
                name: "chk_classes_subject_name_length",
                table: "classes"
            );

            migrationBuilder.DropColumn(name: "subject_name", table: "classes");
        }
    }
}
