using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_questions_and_views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questions",
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
                    type = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    content = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    suggested_point = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    visibility = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    explanation = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
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
                    table.PrimaryKey("pk_questions", x => x.id);
                    table.CheckConstraint(
                        "chk_questions_content_length",
                        "length(content) >= 10 AND length(content) <= 10000"
                    );
                    table.CheckConstraint(
                        "chk_questions_difficulty",
                        "difficulty IN ('Easy', 'Medium', 'Hard')"
                    );
                    table.CheckConstraint(
                        "chk_questions_explanation_length",
                        "explanation IS NULL OR length(explanation) <= 2000"
                    );
                    table.CheckConstraint(
                        "chk_questions_suggested_point",
                        "suggested_point > 0 AND suggested_point <= 100"
                    );
                    table.CheckConstraint(
                        "chk_questions_type",
                        "type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')"
                    );
                    table.CheckConstraint(
                        "chk_questions_visibility",
                        "visibility IN ('Private', 'Public')"
                    );
                    table.ForeignKey(
                        name: "fk_questions_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_questions_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_questions_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_questions_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: false
                    ),
                    is_correct = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_options", x => x.id);
                    table.CheckConstraint(
                        "chk_question_options_content_length",
                        "length(content) >= 1 AND length(content) <= 2000"
                    );
                    table.CheckConstraint(
                        "chk_question_options_display_order",
                        "display_order >= 0"
                    );
                    table.ForeignKey(
                        name: "fk_question_options_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "question_tags",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_question_tags", x => x.id);
                    table.CheckConstraint("chk_question_tags_format", "tag ~ '^[a-z0-9-]{1,50}$'");
                    table.ForeignKey(
                        name: "fk_question_tags_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "uq_question_options_order",
                table: "question_options",
                columns: new[] { "question_id", "display_order" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_question_tags_tag",
                table: "question_tags",
                column: "tag"
            );

            migrationBuilder.CreateIndex(
                name: "uq_question_tags_unique",
                table: "question_tags",
                columns: new[] { "question_id", "tag" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_difficulty",
                table: "questions",
                column: "difficulty"
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_subject",
                table: "questions",
                column: "subject_id",
                filter: "subject_id IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_teacher",
                table: "questions",
                column: "teacher_id",
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_teacher_type",
                table: "questions",
                columns: new[] { "teacher_id", "type", "difficulty" },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_type",
                table: "questions",
                column: "type"
            );

            migrationBuilder.CreateIndex(
                name: "idx_questions_visibility_public",
                table: "questions",
                column: "visibility",
                filter: "visibility = 'Public' AND deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_questions_created_by",
                table: "questions",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_questions_updated_by",
                table: "questions",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_questions_public_id",
                table: "questions",
                column: "public_id",
                unique: true
            );

            // pg_trgm + a functional GIN index on lower(content) so the case-insensitive substring
            // keyword search (lower(content) LIKE '%term%') stays fast as the bank grows.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(
                "CREATE INDEX idx_questions_content_trgm ON questions USING gin (lower(content) gin_trgm_ops);"
            );

            // Read model for question lists/detail: one row per non-deleted question + owner display
            // name + live subject name/public id + option count + aggregated tag array. Lets list
            // queries reuse BaseGetQueryHandler without joining the Identity ApplicationUser type.
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_questions AS
SELECT
    q.id,
    q.public_id,
    q.type,
    q.content,
    q.difficulty,
    q.suggested_point,
    q.visibility,
    q.explanation,
    q.created_at,
    q.updated_at,
    q.subject_id,
    sub.public_id  AS subject_public_id,
    sub.name       AS subject_name,
    q.teacher_id,
    t.public_id    AS teacher_public_id,
    t.display_name AS teacher_name,
    COALESCE(oc.option_count, 0)    AS option_count,
    COALESCE(tg.tags, '{}'::text[]) AS tags
FROM questions q
JOIN users t ON t.id = q.teacher_id
LEFT JOIN subjects sub ON sub.id = q.subject_id AND sub.deleted_at IS NULL
LEFT JOIN (
    SELECT question_id, COUNT(*) AS option_count
    FROM question_options
    GROUP BY question_id
) oc ON oc.question_id = q.id
LEFT JOIN (
    SELECT question_id, array_agg(tag ORDER BY tag) AS tags
    FROM question_tags
    GROUP BY question_id
) tg ON tg.question_id = q.id
WHERE q.deleted_at IS NULL;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the view + functional index first — they depend on the questions table. The
            // pg_trgm extension is left in place (it may be shared by other objects).
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_questions;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_questions_content_trgm;");

            migrationBuilder.DropTable(name: "question_options");

            migrationBuilder.DropTable(name: "question_tags");

            migrationBuilder.DropTable(name: "questions");
        }
    }
}
