using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    normalized_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                    table.CheckConstraint(
                        "chk_roles_name",
                        "name IN ('Student', 'Teacher', 'Admin')"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "users",
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
                    display_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    locked_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    locked_by = table.Column<long>(type: "bigint", nullable: true),
                    last_login_at = table.Column<DateTime>(
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
                    deleted_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    user_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    normalized_user_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    email = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint(
                        "chk_users_avatar_url",
                        "avatar_url IS NULL OR avatar_url ~ '^https?://'"
                    );
                    table.CheckConstraint(
                        "chk_users_bio_length",
                        "bio IS NULL OR length(bio) <= 500"
                    );
                    table.CheckConstraint(
                        "chk_users_display_name_length",
                        "length(display_name) >= 2 AND length(display_name) <= 100"
                    );
                    table.ForeignKey(
                        name: "fk_users_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_users_users_locked_by",
                        column: x => x.locked_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_users_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    used_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                    table.CheckConstraint("chk_prt_expires", "expires_at > created_at");
                    table.ForeignKey(
                        name: "fk_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    revoked_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    replaced_by_token_id = table.Column<long>(type: "bigint", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.CheckConstraint("chk_refresh_tokens_expires", "expires_at > created_at");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_refresh_tokens_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "subjects",
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
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    display_order = table.Column<int>(
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
                    table.PrimaryKey("pk_subjects", x => x.id);
                    table.CheckConstraint(
                        "chk_subjects_description_length",
                        "description IS NULL OR length(description) <= 500"
                    );
                    table.CheckConstraint(
                        "chk_subjects_name_length",
                        "length(name) >= 2 AND length(name) <= 100"
                    );
                    table.ForeignKey(
                        name: "fk_subjects_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_subjects_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_user_logins",
                        x => new { x.login_provider, x.provider_key }
                    );
                    table.ForeignKey(
                        name: "fk_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    assigned_by = table.Column<long>(type: "bigint", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_user_roles_users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_user_tokens",
                        x => new
                        {
                            x.user_id,
                            x.login_provider,
                            x.name,
                        }
                    );
                    table.ForeignKey(
                        name: "fk_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "classes",
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
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    subject_id = table.Column<long>(type: "bigint", nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    invite_code = table.Column<string>(
                        type: "character varying(8)",
                        maxLength: 8,
                        nullable: false
                    ),
                    status = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "Active"
                    ),
                    cover_image_url = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_classes", x => x.id);
                    table.CheckConstraint(
                        "chk_classes_cover_image_url",
                        "cover_image_url IS NULL OR cover_image_url ~ '^https?://'"
                    );
                    table.CheckConstraint(
                        "chk_classes_description_length",
                        "description IS NULL OR length(description) <= 1000"
                    );
                    table.CheckConstraint(
                        "chk_classes_invite_code",
                        "invite_code ~ '^[A-Z0-9]{6,8}$'"
                    );
                    table.CheckConstraint(
                        "chk_classes_name_length",
                        "length(name) >= 2 AND length(name) <= 200"
                    );
                    table.CheckConstraint("chk_classes_status", "status IN ('Active', 'Archived')");
                    table.ForeignKey(
                        name: "fk_classes_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_classes_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_classes_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_classes_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "class_memberships",
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
                    class_id = table.Column<long>(type: "bigint", nullable: false),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "Pending"
                    ),
                    joined_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    processed_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    processed_by = table.Column<long>(type: "bigint", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_class_memberships", x => x.id);
                    table.CheckConstraint(
                        "chk_memberships_rejection_reason",
                        "rejection_reason IS NULL OR length(rejection_reason) <= 500"
                    );
                    table.CheckConstraint(
                        "chk_memberships_status",
                        "status IN ('Pending', 'Approved', 'Rejected', 'Removed', 'Left')"
                    );
                    table.ForeignKey(
                        name: "fk_class_memberships_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_class_memberships_users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_class_memberships_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "idx_memberships_class_approved",
                table: "class_memberships",
                column: "class_id",
                filter: "status = 'Approved'"
            );

            migrationBuilder.CreateIndex(
                name: "idx_memberships_class_status",
                table: "class_memberships",
                columns: new[] { "class_id", "status" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_memberships_student_approved",
                table: "class_memberships",
                columns: new[] { "student_id", "status" },
                filter: "status = 'Approved'"
            );

            migrationBuilder.CreateIndex(
                name: "ix_class_memberships_processed_by",
                table: "class_memberships",
                column: "processed_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_memberships_class_student_active",
                table: "class_memberships",
                columns: new[] { "class_id", "student_id" },
                unique: true,
                filter: "status IN ('Pending', 'Approved')"
            );

            migrationBuilder.CreateIndex(
                name: "uq_memberships_public_id",
                table: "class_memberships",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_classes_owner",
                table: "classes",
                column: "owner_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_classes_status",
                table: "classes",
                columns: new[] { "status", "owner_id" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_classes_subject",
                table: "classes",
                column: "subject_id",
                filter: "subject_id IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_classes_created_by",
                table: "classes",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_classes_updated_by",
                table: "classes",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_classes_invite_code",
                table: "classes",
                column: "invite_code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_classes_name_owner_active",
                table: "classes",
                columns: new[] { "name", "owner_id" },
                unique: true,
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "uq_classes_public_id",
                table: "classes",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_prt_user_active",
                table: "password_reset_tokens",
                column: "user_id",
                filter: "used_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "uq_password_reset_hash",
                table: "password_reset_tokens",
                column: "token_hash",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_expires",
                table: "refresh_tokens",
                column: "expires_at"
            );

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_active",
                table: "refresh_tokens",
                columns: new[] { "user_id", "revoked_at" },
                filter: "revoked_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_replaced_by_token_id",
                table: "refresh_tokens",
                column: "replaced_by_token_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_refresh_tokens_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                table: "role_claims",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "normalized_name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_subjects_display_order",
                table: "subjects",
                column: "display_order"
            );

            migrationBuilder.CreateIndex(
                name: "idx_subjects_is_active",
                table: "subjects",
                column: "is_active",
                filter: "is_active = true"
            );

            migrationBuilder.CreateIndex(
                name: "ix_subjects_created_by",
                table: "subjects",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_subjects_updated_by",
                table: "subjects",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_subjects_name_active",
                table: "subjects",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "uq_subjects_public_id",
                table: "subjects",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                table: "user_claims",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "idx_user_roles_role_id",
                table: "user_roles",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_assigned_by",
                table: "user_roles",
                column: "assigned_by"
            );

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email"
            );

            migrationBuilder.CreateIndex(
                name: "idx_users_deleted_at",
                table: "users",
                column: "deleted_at"
            );

            migrationBuilder.CreateIndex(
                name: "idx_users_is_locked",
                table: "users",
                column: "is_locked",
                filter: "is_locked = true"
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_created_by",
                table: "users",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_locked_by",
                table: "users",
                column: "locked_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_updated_by",
                table: "users",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "uq_users_email_active",
                table: "users",
                column: "email",
                unique: true,
                filter: "deleted_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "uq_users_public_id",
                table: "users",
                column: "public_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "class_memberships");

            migrationBuilder.DropTable(name: "password_reset_tokens");

            migrationBuilder.DropTable(name: "refresh_tokens");

            migrationBuilder.DropTable(name: "role_claims");

            migrationBuilder.DropTable(name: "user_claims");

            migrationBuilder.DropTable(name: "user_logins");

            migrationBuilder.DropTable(name: "user_roles");

            migrationBuilder.DropTable(name: "user_tokens");

            migrationBuilder.DropTable(name: "classes");

            migrationBuilder.DropTable(name: "roles");

            migrationBuilder.DropTable(name: "subjects");

            migrationBuilder.DropTable(name: "users");
        }
    }
}
