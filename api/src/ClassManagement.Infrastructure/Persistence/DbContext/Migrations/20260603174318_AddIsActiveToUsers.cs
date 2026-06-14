using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassManagement.Infrastructure.Persistence.DbContext.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_users_is_active",
                table: "users",
                column: "is_active",
                filter: "is_active = false"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "idx_users_is_active", table: "users");

            migrationBuilder.DropColumn(name: "is_active", table: "users");
        }
    }
}
