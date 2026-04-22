using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitMetrics_AppAndSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RamUsage",
                table: "HealthSnapshots",
                newName: "SystemRamUsage");

            migrationBuilder.RenameColumn(
                name: "CpuUsage",
                table: "HealthSnapshots",
                newName: "SystemCpuUsage");

            migrationBuilder.AddColumn<double>(
                name: "AppCpuUsage",
                table: "HealthSnapshots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AppRamUsage",
                table: "HealthSnapshots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppCpuUsage",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "AppRamUsage",
                table: "HealthSnapshots");

            migrationBuilder.RenameColumn(
                name: "SystemRamUsage",
                table: "HealthSnapshots",
                newName: "RamUsage");

            migrationBuilder.RenameColumn(
                name: "SystemCpuUsage",
                table: "HealthSnapshots",
                newName: "CpuUsage");
        }
    }
}
