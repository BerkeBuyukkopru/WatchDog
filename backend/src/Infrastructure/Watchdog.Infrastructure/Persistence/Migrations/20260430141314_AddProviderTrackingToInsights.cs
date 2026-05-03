using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderTrackingToInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "AiInsights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "AiInsights",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "AiInsights");
        }
    }
}
