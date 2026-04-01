using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxRamThresholdMb",
                table: "SystemConfigurations",
                newName: "CriticalRamThreshold");

            migrationBuilder.RenameColumn(
                name: "RamUsageMb",
                table: "HealthSnapshots",
                newName: "RamUsage");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriticalRamThreshold",
                value: 90.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CriticalRamThreshold",
                table: "SystemConfigurations",
                newName: "MaxRamThresholdMb");

            migrationBuilder.RenameColumn(
                name: "RamUsage",
                table: "HealthSnapshots",
                newName: "RamUsageMb");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                column: "MaxRamThresholdMb",
                value: 2048.0);
        }
    }
}
