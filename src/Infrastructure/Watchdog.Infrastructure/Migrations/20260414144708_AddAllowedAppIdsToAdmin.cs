using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedAppIdsToAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedAppIds",
                table: "AdminUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedAppIds",
                table: "AdminUsers");
        }
    }
}
