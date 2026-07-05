using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_proctoring_to_assignments_attempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs"
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_flagged",
                table: "attempts",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                table: "attempts",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "last_event_at",
                table: "attempts",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "violation_count",
                table: "attempts",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<bool>(
                name: "block_copy_paste",
                table: "assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "detect_tab_switch",
                table: "assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "max_violations",
                table: "assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<bool>(
                name: "require_fullscreen",
                table: "assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "violation_action",
                table: "assignments",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "WarnOnly"
            );

            migrationBuilder.CreateTable(
                name: "attempt_events",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    occurred_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_events", x => x.id);
                    table.CheckConstraint(
                        "chk_attempt_events_type",
                        "event_type IN ('TabSwitch', 'FocusLoss', 'FullscreenExit', 'CopyAttempt', 'PasteAttempt', 'ContextMenu', 'DevToolsOpen', 'ReloadAttempt', 'InactivityTimeout')"
                    );
                    table.ForeignKey(
                        name: "fk_attempt_events_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs",
                sql: "action IN ('user.login', 'user.logout', 'user.login_failed', 'user.password_changed', 'user.password_reset_requested', 'user.password_reset_completed', 'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted', 'question.created', 'question.updated', 'question.correct_answer_changed', 'question.deleted', 'question.visibility_changed', 'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed', 'assignment.published', 'assignment.closed', 'assignment.archived', 'attempt.started', 'attempt.submitted', 'attempt.auto_submitted', 'attempt.force_submitted', 'attempt.flagged', 'attempt.unflagged', 'attempt.unlocked', 'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published', 'report.created', 'report.reviewed', 'report.resolved', 'report.rejected', 'admin.system_settings_changed', 'admin.assignment_force_closed', 'subscription.created', 'subscription.cancelled', 'subscription.reactivated', 'subscription.manual_set', 'subscription.expired', 'payment.created', 'payment.completed', 'payment.failed', 'invoice.issued')"
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempts_flagged",
                table: "attempts",
                column: "assignment_id",
                filter: "is_flagged = true"
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_assignments_max_violations",
                table: "assignments",
                sql: "max_violations >= 0"
            );

            migrationBuilder.AddCheckConstraint(
                name: "chk_assignments_violation_action",
                table: "assignments",
                sql: "violation_action IN ('WarnOnly', 'AutoSubmit', 'LockAttempt')"
            );

            migrationBuilder.CreateIndex(
                name: "idx_attempt_events_attempt",
                table: "attempt_events",
                columns: new[] { "attempt_id", "occurred_at" }
            );

            // Recreate vw_assignments to surface the proctoring config (MVP-10) for teacher/admin detail.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_assignments;");
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_assignments AS
                SELECT
                    a.id, a.public_id, a.title, a.description, a.status,
                    a.opens_at, a.closes_at, a.time_limit_minutes, a.max_attempts,
                    a.score_policy, a.allow_late, a.grade_publish_policy,
                    a.shuffle_questions, a.shuffle_options, a.show_answers_after_grade,
                    a.require_fullscreen, a.detect_tab_switch, a.block_copy_paste,
                    a.max_violations, a.violation_action,
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

            // Recreate vw_attempts to surface the proctoring integrity summary (MVP-10).
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_attempts;");
            migrationBuilder.Sql(
                """
                CREATE VIEW vw_attempts AS
                SELECT
                    at.id, at.public_id, at.status, at.attempt_number,
                    at.started_at, at.submitted_at, at.deadline_at, at.auto_submitted,
                    at.violation_count, at.is_flagged, at.is_locked,
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
            // Restore the pre-MVP-10 views first — they reference the columns dropped below.
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_assignments;");
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

            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_attempts;");
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

            migrationBuilder.DropTable(name: "attempt_events");

            migrationBuilder.DropCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs"
            );

            migrationBuilder.DropIndex(name: "idx_attempts_flagged", table: "attempts");

            migrationBuilder.DropCheckConstraint(
                name: "chk_assignments_max_violations",
                table: "assignments"
            );

            migrationBuilder.DropCheckConstraint(
                name: "chk_assignments_violation_action",
                table: "assignments"
            );

            migrationBuilder.DropColumn(name: "is_flagged", table: "attempts");

            migrationBuilder.DropColumn(name: "is_locked", table: "attempts");

            migrationBuilder.DropColumn(name: "last_event_at", table: "attempts");

            migrationBuilder.DropColumn(name: "violation_count", table: "attempts");

            migrationBuilder.DropColumn(name: "block_copy_paste", table: "assignments");

            migrationBuilder.DropColumn(name: "detect_tab_switch", table: "assignments");

            migrationBuilder.DropColumn(name: "max_violations", table: "assignments");

            migrationBuilder.DropColumn(name: "require_fullscreen", table: "assignments");

            migrationBuilder.DropColumn(name: "violation_action", table: "assignments");

            migrationBuilder.AddCheckConstraint(
                name: "chk_audit_logs_action",
                table: "audit_logs",
                sql: "action IN ('user.login', 'user.logout', 'user.login_failed', 'user.password_changed', 'user.password_reset_requested', 'user.password_reset_completed', 'user.role_changed', 'user.locked', 'user.unlocked', 'user.deleted', 'question.created', 'question.updated', 'question.correct_answer_changed', 'question.deleted', 'question.visibility_changed', 'exam.created', 'exam.updated', 'exam.deleted', 'exam.visibility_changed', 'assignment.published', 'assignment.closed', 'assignment.archived', 'attempt.started', 'attempt.submitted', 'attempt.auto_submitted', 'grade.manual_graded', 'grade.manual_grade_updated', 'grade.published', 'report.created', 'report.reviewed', 'report.resolved', 'report.rejected', 'admin.system_settings_changed', 'admin.assignment_force_closed', 'subscription.created', 'subscription.cancelled', 'subscription.reactivated', 'subscription.manual_set', 'subscription.expired', 'payment.created', 'payment.completed', 'payment.failed', 'invoice.issued')"
            );
        }
    }
}
