using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class create_manual_grades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manual_grades",
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
                    score = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    feedback = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_manual_grades", x => x.id);
                    table.CheckConstraint(
                        "chk_manual_grades_feedback_length",
                        "feedback IS NULL OR length(feedback) <= 5000"
                    );
                    table.CheckConstraint("chk_manual_grades_score", "score >= 0");
                    table.ForeignKey(
                        name: "fk_manual_grades_asp_net_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_manual_grades_asp_net_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_manual_grades_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_manual_grades_snapshot_questions_snapshot_question_id",
                        column: x => x.snapshot_question_id,
                        principalTable: "snapshot_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_manual_grades_attempt",
                table: "manual_grades",
                column: "attempt_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_manual_grades_graded_by",
                table: "manual_grades",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_manual_grades_snapshot_question_id",
                table: "manual_grades",
                column: "snapshot_question_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_manual_grades_updated_by",
                table: "manual_grades",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_manual_grades_attempt_question",
                table: "manual_grades",
                columns: new[] { "attempt_id", "snapshot_question_id" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "manual_grades");
        }
    }
}
