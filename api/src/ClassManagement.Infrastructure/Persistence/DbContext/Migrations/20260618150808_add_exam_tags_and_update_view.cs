using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_exam_tags_and_update_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_tags",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    exam_id = table.Column<long>(type: "bigint", nullable: false),
                    tag = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_tags", x => x.id);
                    table.CheckConstraint("chk_exam_tags_format", "tag ~ '^[a-z0-9-]{1,50}$'");
                    table.ForeignKey(
                        name: "fk_exam_tags_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_exam_tags_tag",
                table: "exam_tags",
                column: "tag"
            );

            migrationBuilder.CreateIndex(
                name: "uq_exam_tags_unique",
                table: "exam_tags",
                columns: new[] { "exam_id", "tag" },
                unique: true
            );

            // Recreate vw_exams with the aggregated tags (text[]) column.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_exams;");
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_exams AS
SELECT
    e.id,
    e.public_id,
    e.title,
    e.description,
    e.visibility,
    e.version,
    e.total_point,
    e.total_questions,
    e.created_at,
    e.updated_at,
    e.subject_id,
    sub.public_id  AS subject_public_id,
    sub.name       AS subject_name,
    e.teacher_id,
    t.public_id    AS teacher_public_id,
    t.display_name AS teacher_name,
    COALESCE(tg.tags, '{}'::text[]) AS tags
FROM exams e
JOIN users t ON t.id = e.teacher_id
LEFT JOIN subjects sub ON sub.id = e.subject_id AND sub.deleted_at IS NULL
LEFT JOIN (
    SELECT exam_id, array_agg(tag ORDER BY tag) AS tags
    FROM exam_tags
    GROUP BY exam_id
) tg ON tg.exam_id = e.id
WHERE e.deleted_at IS NULL;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the tag-less view first (it references exam_tags via the join), then drop the table.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_exams;");
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_exams AS
SELECT
    e.id,
    e.public_id,
    e.title,
    e.description,
    e.visibility,
    e.version,
    e.total_point,
    e.total_questions,
    e.created_at,
    e.updated_at,
    e.subject_id,
    sub.public_id  AS subject_public_id,
    sub.name       AS subject_name,
    e.teacher_id,
    t.public_id    AS teacher_public_id,
    t.display_name AS teacher_name
FROM exams e
JOIN users t ON t.id = e.teacher_id
LEFT JOIN subjects sub ON sub.id = e.subject_id AND sub.deleted_at IS NULL
WHERE e.deleted_at IS NULL;
"
            );

            migrationBuilder.DropTable(name: "exam_tags");
        }
    }
}
