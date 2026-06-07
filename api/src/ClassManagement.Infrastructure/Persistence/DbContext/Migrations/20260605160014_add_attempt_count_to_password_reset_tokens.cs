using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class add_attempt_count_to_password_reset_tokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                table: "password_reset_tokens",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "attempt_count", table: "password_reset_tokens");
        }
    }
}
