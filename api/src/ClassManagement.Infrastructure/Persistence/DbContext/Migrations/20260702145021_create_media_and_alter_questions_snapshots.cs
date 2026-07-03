using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_media_and_alter_questions_snapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "media_id",
                table: "question_options",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "max_storage_bytes",
                table: "plans",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "media_assets",
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
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    storage_key = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    url = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: false
                    ),
                    kind = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    content_type = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    byte_size = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Pending"
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
                    table.PrimaryKey("pk_media_assets", x => x.id);
                    table.CheckConstraint("chk_media_assets_byte_size", "byte_size > 0");
                    table.CheckConstraint(
                        "chk_media_assets_dimensions",
                        "(width IS NULL OR width > 0) AND (height IS NULL OR height > 0) AND (duration_seconds IS NULL OR duration_seconds >= 0)"
                    );
                    table.CheckConstraint(
                        "chk_media_assets_kind",
                        "kind IN ('Image', 'Audio', 'Video')"
                    );
                    table.CheckConstraint(
                        "chk_media_assets_provider",
                        "provider IN ('Local', 'R2')"
                    );
                    table.CheckConstraint(
                        "chk_media_assets_status",
                        "status IN ('Pending', 'Confirmed')"
                    );
                    table.ForeignKey(
                        name: "fk_media_assets_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_media_assets_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_media_assets_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "snapshot_media",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    snapshot_question_id = table.Column<long>(type: "bigint", nullable: false),
                    snapshot_option_id = table.Column<long>(type: "bigint", nullable: true),
                    media_public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    frozen_url = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: false
                    ),
                    kind = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    role = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    snapshot_created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_snapshot_media", x => x.id);
                    table.CheckConstraint("chk_snapshot_media_display_order", "display_order >= 0");
                    table.CheckConstraint(
                        "chk_snapshot_media_kind",
                        "kind IN ('Image', 'Audio', 'Video')"
                    );
                    table.CheckConstraint(
                        "chk_snapshot_media_role",
                        "role IN ('Inline', 'Attachment')"
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_media_snapshot_options_snapshot_option_id",
                        column: x => x.snapshot_option_id,
                        principalTable: "snapshot_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_media_snapshot_questions_snapshot_question_id",
                        column: x => x.snapshot_question_id,
                        principalTable: "snapshot_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "question_media",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    media_id = table.Column<long>(type: "bigint", nullable: false),
                    role = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
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
                    table.PrimaryKey("pk_question_media", x => x.id);
                    table.CheckConstraint("chk_question_media_display_order", "display_order >= 0");
                    table.CheckConstraint(
                        "chk_question_media_role",
                        "role IN ('Inline', 'Attachment')"
                    );
                    table.ForeignKey(
                        name: "fk_question_media_media_assets_media_id",
                        column: x => x.media_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_question_media_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_question_options_media",
                table: "question_options",
                column: "media_id"
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_plans_max_storage_bytes",
                table: "plans",
                sql: "max_storage_bytes IS NULL OR max_storage_bytes > 0"
            );

            migrationBuilder.CreateIndex(
                name: "idx_media_assets_deleted",
                table: "media_assets",
                column: "deleted_at",
                filter: "deleted_at IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_media_assets_owner",
                table: "media_assets",
                column: "owner_id",
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_media_assets_status_created",
                table: "media_assets",
                columns: new[] { "status", "created_at" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_created_by",
                table: "media_assets",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_updated_by",
                table: "media_assets",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_media_assets_public_id",
                table: "media_assets",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_question_media_media",
                table: "question_media",
                column: "media_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_question_media_order",
                table: "question_media",
                columns: new[] { "question_id", "display_order" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_snapshot_media_public_id",
                table: "snapshot_media",
                column: "media_public_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_snapshot_media_question",
                table: "snapshot_media",
                column: "snapshot_question_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_snapshot_media_snapshot_option_id",
                table: "snapshot_media",
                column: "snapshot_option_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_question_options_media_assets_media_id",
                table: "question_options",
                column: "media_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull
            );

            // Recreate vw_questions to add media_count (number of question-level attachments). MVP-9.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_questions;");
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
    COALESCE(mc.media_count, 0)     AS media_count,
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
    SELECT question_id, COUNT(*) AS media_count
    FROM question_media
    GROUP BY question_id
) mc ON mc.question_id = q.id
LEFT JOIN (
    SELECT question_id, array_agg(tag ORDER BY tag) AS tags
    FROM question_tags
    GROUP BY question_id
) tg ON tg.question_id = q.id
WHERE q.deleted_at IS NULL;
"
            );

            // vw_media — teacher library + admin storage lists. Omits storage_key (BR-9-01).
            migrationBuilder.Sql(
                @"
CREATE VIEW vw_media AS
SELECT
    m.id,
    m.public_id,
    m.provider,
    m.url,
    m.kind,
    m.content_type,
    m.byte_size,
    m.width,
    m.height,
    m.duration_seconds,
    m.status,
    m.created_at,
    m.updated_at,
    m.owner_id,
    u.public_id    AS owner_public_id,
    u.display_name AS owner_name
FROM media_assets m
JOIN users u ON u.id = m.owner_id
WHERE m.deleted_at IS NULL;
"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the media-aware views first — they depend on question_media / media_assets.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_media;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_questions;");

            migrationBuilder.DropForeignKey(
                name: "fk_question_options_media_assets_media_id",
                table: "question_options"
            );

            migrationBuilder.DropTable(name: "question_media");

            migrationBuilder.DropTable(name: "snapshot_media");

            migrationBuilder.DropTable(name: "media_assets");

            migrationBuilder.DropIndex(
                name: "idx_question_options_media",
                table: "question_options"
            );

            migrationBuilder.DropCheckConstraint(
                name: "chk_plans_max_storage_bytes",
                table: "plans"
            );

            migrationBuilder.DropColumn(name: "media_id", table: "question_options");

            migrationBuilder.DropColumn(name: "max_storage_bytes", table: "plans");

            // Restore the pre-MVP-9 vw_questions (without media_count), now that question_media is gone.
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
    }
}
