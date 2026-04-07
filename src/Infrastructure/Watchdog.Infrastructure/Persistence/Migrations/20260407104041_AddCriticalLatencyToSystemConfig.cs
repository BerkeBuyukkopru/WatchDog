using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCriticalLatencyToSystemConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CriticalLatencyThreshold",
                table: "SystemConfigurations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriticalLatencyThreshold",
                value: 1000.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriticalLatencyThreshold",
                table: "SystemConfigurations");
        }
    }
}
