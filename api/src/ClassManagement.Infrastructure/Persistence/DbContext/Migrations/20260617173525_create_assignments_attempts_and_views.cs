using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_assignments_attempts_and_views : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assignments",
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
                    exam_id = table.Column<long>(type: "bigint", nullable: false),
                    exam_version_at_publish = table.Column<int>(type: "integer", nullable: true),
                    class_id = table.Column<long>(type: "bigint", nullable: false),
                    teacher_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: true
                    ),
                    opens_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    closes_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: true),
                    max_attempts = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 1
                    ),
                    score_policy = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    allow_late = table.Column<bool>(type: "boolean", nullable: false),
                    grade_publish_policy = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    shuffle_questions = table.Column<bool>(type: "boolean", nullable: false),
                    shuffle_options = table.Column<bool>(type: "boolean", nullable: false),
                    show_answers_after_grade = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    published_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    closed_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    grades_released_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
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
                    table.PrimaryKey("pk_assignments", x => x.id);
                    table.CheckConstraint(
                        "chk_assignments_description_length",
                        "description IS NULL OR length(description) <= 1000"
                    );
                    table.CheckConstraint(
                        "chk_assignments_grade_publish_policy",
                        "grade_publish_policy IN ('Immediate', 'AfterDeadline', 'Manual')"
                    );
                    table.CheckConstraint(
                        "chk_assignments_max_attempts",
                        "max_attempts >= 1 AND max_attempts <= 100"
                    );
                    table.CheckConstraint(
                        "chk_assignments_score_policy",
                        "score_policy IN ('Highest', 'Latest')"
                    );
                    table.CheckConstraint(
                        "chk_assignments_status",
                        "status IN ('Draft', 'Scheduled', 'Open', 'Closed', 'Archived')"
                    );
                    table.CheckConstraint(
                        "chk_assignments_time_limit",
                        "time_limit_minutes IS NULL OR (time_limit_minutes > 0 AND time_limit_minutes <= 1440)"
                    );
                    table.CheckConstraint(
                        "chk_assignments_title_length",
                        "length(title) >= 3 AND length(title) <= 300"
                    );
                    table.CheckConstraint(
                        "chk_assignments_window",
                        "opens_at IS NULL OR closes_at IS NULL OR closes_at > opens_at"
                    );
                    table.ForeignKey(
                        name: "fk_assignments_asp_net_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_assignments_asp_net_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_assignments_asp_net_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_assignments_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_assignments_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "assignment_snapshots",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    total_point = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_assignment_snapshots", x => x.id);
                    table.CheckConstraint(
                        "chk_assignment_snapshots_total_point",
                        "total_point > 0"
                    );
                    table.CheckConstraint(
                        "chk_assignment_snapshots_total_questions",
                        "total_questions > 0"
                    );
                    table.ForeignKey(
                        name: "fk_assignment_snapshots_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "attempts",
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
                    assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    started_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    submitted_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    deadline_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    auto_submitted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    question_order = table.Column<string>(type: "jsonb", nullable: false),
                    total_auto_score = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    total_manual_score = table.Column<decimal>(
                        type: "numeric(8,2)",
                        nullable: true
                    ),
                    total_score = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_attempts", x => x.id);
                    table.CheckConstraint("chk_attempts_number", "attempt_number >= 1");
                    table.CheckConstraint(
                        "chk_attempts_status",
                        "status IN ('InProgress', 'Submitted', 'AutoGraded', 'NeedManualGrading', 'Graded')"
                    );
                    table.CheckConstraint(
                        "chk_attempts_submitted_after_started",
                        "submitted_at IS NULL OR submitted_at >= started_at"
                    );
                    table.CheckConstraint(
                        "chk_attempts_total_auto_score",
                        "total_auto_score IS NULL OR total_auto_score >= 0"
                    );
                    table.CheckConstraint(
                        "chk_attempts_total_manual_score",
                        "total_manual_score IS NULL OR total_manual_score >= 0"
                    );
                    table.CheckConstraint(
                        "chk_attempts_total_score",
                        "total_score IS NULL OR total_score >= 0"
                    );
                    table.ForeignKey(
                        name: "fk_attempts_asp_net_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_attempts_asp_net_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_attempts_asp_net_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_attempts_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "snapshot_questions",
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
                    snapshot_id = table.Column<long>(type: "bigint", nullable: false),
                    original_question_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    content = table.Column<string>(type: "text", nullable: false),
                    point = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_snapshot_questions", x => x.id);
                    table.CheckConstraint(
                        "chk_snapshot_questions_display_order",
                        "display_order >= 1"
                    );
                    table.CheckConstraint("chk_snapshot_questions_point", "point > 0");
                    table.CheckConstraint(
                        "chk_snapshot_questions_type",
                        "type IN ('SingleChoice', 'MultipleChoice', 'TrueFalse', 'ShortWriting', 'LongWriting')"
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_questions_assignment_snapshots_snapshot_id",
                        column: x => x.snapshot_id,
                        principalTable: "assignment_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_questions_questions_original_question_id",
                        column: x => x.original_question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "attempt_answers",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    snapshot_question_id = table.Column<long>(type: "bigint", nullable: false),
                    selected_option_ids = table.Column<string>(type: "jsonb", nullable: true),
                    text_answer = table.Column<string>(type: "text", nullable: true),
                    auto_score = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    is_auto_graded = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    last_saved_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    submitted_at = table.Column<DateTime>(
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
                    table.PrimaryKey("pk_attempt_answers", x => x.id);
                    table.CheckConstraint(
                        "chk_attempt_answers_auto_score",
                        "auto_score IS NULL OR auto_score >= 0"
                    );
                    table.CheckConstraint(
                        "chk_attempt_answers_text_length",
                        "text_answer IS NULL OR length(text_answer) <= 50000"
                    );
                    table.ForeignKey(
                        name: "fk_attempt_answers_asp_net_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_attempt_answers_asp_net_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_attempt_answers_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_attempt_answers_snapshot_questions_snapshot_question_id",
                        column: x => x.snapshot_question_id,
                        principalTable: "snapshot_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "snapshot_options",
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
                    snapshot_question_id = table.Column<long>(type: "bigint", nullable: false),
                    original_option_id = table.Column<long>(type: "bigint", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_snapshot_options", x => x.id);
                    table.CheckConstraint(
                        "chk_snapshot_options_display_order",
                        "display_order >= 0"
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_options_question_options_original_option_id",
                        column: x => x.original_option_id,
                        principalTable: "question_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_snapshot_options_snapshot_questions_snapshot_question_id",
                        column: x => x.snapshot_question_id,
                        principalTable: "snapshot_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "uq_assignment_snapshots_assignment",
                table: "assignment_snapshots",
                column: "assignment_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_assignments_class_status_opens",
                table: "assignments",
                columns: new[] { "class_id", "status", "opens_at" },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_assignments_closes_at",
                table: "assignments",
                column: "closes_at",
                filter: "status = 'Open' AND deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_assignments_exam",
                table: "assignments",
                column: "exam_id",
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_assignments_scheduled",
                table: "assignments",
                column: "opens_at",
                filter: "status = 'Scheduled' AND deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "idx_assignments_teacher",
                table: "assignments",
                columns: new[] { "teacher_id", "status" },
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_assignments_created_by",
                table: "assignments",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_assignments_updated_by",
                table: "assignments",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_assignments_public_id",
                table: "assignments",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempt_answers_attempt",
                table: "attempt_answers",
                column: "attempt_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_created_by",
                table: "attempt_answers",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_snapshot_question_id",
                table: "attempt_answers",
                column: "snapshot_question_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_attempt_answers_updated_by",
                table: "attempt_answers",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_attempt_answers_unique",
                table: "attempt_answers",
                columns: new[] { "attempt_id", "snapshot_question_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempts_assignment_status",
                table: "attempts",
                columns: new[] { "assignment_id", "status" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempts_deadline",
                table: "attempts",
                column: "deadline_at",
                filter: "status = 'InProgress'"
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempts_student_assignment",
                table: "attempts",
                columns: new[] { "student_id", "assignment_id" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_attempts_created_by",
                table: "attempts",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_attempts_updated_by",
                table: "attempts",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_attempts_one_in_progress",
                table: "attempts",
                columns: new[] { "assignment_id", "student_id" },
                unique: true,
                filter: "status = 'InProgress'"
            );

            migrationBuilder.CreateIndex(
                name: "uq_attempts_public_id",
                table: "attempts",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_attempts_sequence",
                table: "attempts",
                columns: new[] { "assignment_id", "student_id", "attempt_number" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_snapshot_options_original_option_id",
                table: "snapshot_options",
                column: "original_option_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_snapshot_options_order",
                table: "snapshot_options",
                columns: new[] { "snapshot_question_id", "display_order" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_snapshot_options_public_id",
                table: "snapshot_options",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_snapshot_questions_original",
                table: "snapshot_questions",
                column: "original_question_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_snapshot_questions_order",
                table: "snapshot_questions",
                columns: new[] { "snapshot_id", "display_order" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_snapshot_questions_public_id",
                table: "snapshot_questions",
                column: "public_id",
                unique: true
            );

            // ── Read-model views (created via raw SQL; EF maps their shape via ToView) ──

            // vw_assignments — teacher/admin lists + detail: assignment + class/exam/owner names +
            // snapshot totals + attempt counts. Excludes soft-deleted assignments.
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_assignments AS
                SELECT
                    a.id, a.public_id, a.title, a.description, a.status,
                    a.opens_at, a.closes_at, a.time_limit_minutes, a.max_attempts,
                    a.score_policy, a.allow_late, a.grade_publish_policy,
                    a.shuffle_questions, a.shuffle_options, a.show_answers_after_grade,
                    a.published_at, a.closed_at, a.grades_released_at, a.exam_version_at_publish,
                    a.created_at, a.updated_at,
                    a.exam_id, e.public_id AS exam_public_id, e.title AS exam_title,
                    a.class_id, c.public_id AS class_public_id, c.name AS class_name,
                    a.teacher_id, t.public_id AS teacher_public_id, t.display_name AS teacher_name,
                    s.total_point AS total_point, s.total_questions AS total_questions,
                    COALESCE(att.attempt_count, 0)::int AS attempt_count,
                    COALESCE(att.submitted_count, 0)::int AS submitted_count
                FROM assignments a
                JOIN exams e ON e.id = a.exam_id
                JOIN classes c ON c.id = a.class_id
                JOIN users t ON t.id = a.teacher_id
                LEFT JOIN assignment_snapshots s ON s.assignment_id = a.id
                LEFT JOIN (
                    SELECT assignment_id,
                           COUNT(*) AS attempt_count,
                           COUNT(*) FILTER (WHERE status <> 'InProgress') AS submitted_count
                    FROM attempts
                    GROUP BY assignment_id
                ) att ON att.assignment_id = a.id
                WHERE a.deleted_at IS NULL;
                """
            );

            // vw_student_assignments — one row per (published assignment, approved student) with that
            // student's attempt stats. A student only sees assignments of classes they're approved in.
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_student_assignments AS
                SELECT
                    a.id, a.public_id, a.title, a.description, a.status,
                    a.opens_at, a.closes_at, a.time_limit_minutes, a.max_attempts, a.allow_late,
                    a.grade_publish_policy, a.created_at,
                    c.public_id AS class_public_id, c.name AS class_name,
                    t.display_name AS teacher_name,
                    s.total_point AS total_point, s.total_questions AS total_questions,
                    m.student_id AS student_id, u.public_id AS student_public_id,
                    COALESCE(st.used_attempts, 0)::int AS used_attempts,
                    COALESCE(st.has_in_progress, false) AS has_in_progress,
                    st.in_progress_attempt_public_id,
                    st.best_score
                FROM assignments a
                JOIN classes c ON c.id = a.class_id
                JOIN users t ON t.id = a.teacher_id
                JOIN class_memberships m ON m.class_id = a.class_id AND m.status = 'Approved'
                JOIN users u ON u.id = m.student_id
                LEFT JOIN assignment_snapshots s ON s.assignment_id = a.id
                LEFT JOIN (
                    SELECT assignment_id, student_id,
                           COUNT(*) AS used_attempts,
                           bool_or(status = 'InProgress') AS has_in_progress,
                           (array_agg(public_id) FILTER (WHERE status = 'InProgress'))[1]
                               AS in_progress_attempt_public_id,
                           MAX(total_score) AS best_score
                    FROM attempts
                    GROUP BY assignment_id, student_id
                ) st ON st.assignment_id = a.id AND st.student_id = m.student_id
                WHERE a.deleted_at IS NULL
                  AND a.status IN ('Scheduled', 'Open', 'Closed');
                """
            );

            // vw_attempts — one row per attempt for teacher rosters + student history.
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_attempts AS
                SELECT
                    at.id, at.public_id, at.status, at.attempt_number,
                    at.started_at, at.submitted_at, at.deadline_at, at.auto_submitted,
                    at.total_auto_score, at.total_manual_score, at.total_score, at.created_at,
                    at.assignment_id, a.public_id AS assignment_public_id, a.title AS assignment_title,
                    s.total_point AS total_point,
                    at.student_id, u.public_id AS student_public_id, u.display_name AS student_name
                FROM attempts at
                JOIN assignments a ON a.id = at.assignment_id
                JOIN users u ON u.id = at.student_id
                LEFT JOIN assignment_snapshots s ON s.assignment_id = a.id
                WHERE a.deleted_at IS NULL;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop views first — they depend on the tables below.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_attempts;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_student_assignments;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_assignments;");

            migrationBuilder.DropTable(name: "attempt_answers");

            migrationBuilder.DropTable(name: "snapshot_options");

            migrationBuilder.DropTable(name: "attempts");

            migrationBuilder.DropTable(name: "snapshot_questions");

            migrationBuilder.DropTable(name: "assignment_snapshots");

            migrationBuilder.DropTable(name: "assignments");
        }
    }
}
