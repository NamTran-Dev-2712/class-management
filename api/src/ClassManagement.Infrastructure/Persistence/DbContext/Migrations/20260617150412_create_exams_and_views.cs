using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_exams_and_views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    public_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    teacher_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: true
                    ),
                    visibility = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    total_point = table.Column<decimal>(
                        type: "numeric(8,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    total_questions = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    deleted_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exams", x => x.id);
                    table.CheckConstraint(
                        "chk_exams_description_length",
                        "description IS NULL OR length(description) <= 1000"
                    );
                    table.CheckConstraint(
                        "chk_exams_title_length",
                        "length(title) >= 3 AND length(title) <= 300"
                    );
                    table.CheckConstraint("chk_exams_total_point", "total_point >= 0");
                    table.CheckConstraint("chk_exams_total_questions", "total_questions >= 0");
                    table.CheckConstraint("chk_exams_version", "version >= 1");
                    table.CheckConstraint(
                        "chk_exams_visibility",
                        "visibility IN ('Private', 'Public')"
                    );
                    table.ForeignKey(
                        name: "fk_exams_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_exams_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_exams_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_exams_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "exam_questions",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    exam_id = table.Column<long>(type: "bigint", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    point = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_questions", x => x.id);
                    table.CheckConstraint("chk_exam_questions_display_order", "display_order >= 1");
                    table.CheckConstraint("chk_exam_questions_point", "point > 0 AND point <= 100");
                    table.ForeignKey(
                        name: "fk_exam_questions_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_exam_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_exam_questions_question",
                table: "exam_questions",
                column: "question_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_exam_questions_order",
                table: "exam_questions",
                columns: new[] { "exam_id", "display_order" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_exam_questions_unique",
                table: "exam_questions",
                columns: new[] { "exam_id", "question_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_exams_subject",
                table: "exams",
                column: "subject_id",
                filter: "subject_id IS NOT NULL AND deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_exams_teacher",
                table: "exams",
                column: "teacher_id",
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_exams_teacher_visibility",
                table: "exams",
                columns: new[] { "teacher_id", "visibility" },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_exams_visibility_public",
                table: "exams",
                column: "visibility",
                filter: "visibility = 'Public' AND deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_exams_created_by",
                table: "exams",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_exams_updated_by",
                table: "exams",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_exams_public_id",
                table: "exams",
                column: "public_id",
                unique: true
            );

            // Read model for exam lists/detail: one row per non-deleted exam + owner display name +
            // live subject name/public id. Lets list queries reuse BaseGetQueryHandler without joining
            // the Identity ApplicationUser type. Totals come straight from the denormalized columns
            // (maintained in the application write handler).
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the view first — it depends on the exams table.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_exams;");

            migrationBuilder.DropTable(name: "exam_questions");

            migrationBuilder.DropTable(name: "exams");
        }
    }
}
