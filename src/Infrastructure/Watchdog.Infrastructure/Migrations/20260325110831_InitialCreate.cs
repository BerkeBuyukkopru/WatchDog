using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HealthUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PollingIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActiveAiProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiApiUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriticalCpuThreshold = table.Column<double>(type: "float", nullable: false),
                    MaxRamThresholdMb = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsightType = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiInsights_MonitoredApps_AppId",
                        column: x => x.AppId,
                        principalTable: "MonitoredApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalDuration = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CpuUsage = table.Column<double>(type: "float", nullable: false),
                    RamUsageMb = table.Column<double>(type: "float", nullable: false),
                    FreeDiskGb = table.Column<double>(type: "float", nullable: false),
                    DependencyDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthSnapshots_MonitoredApps_AppId",
                        column: x => x.AppId,
                        principalTable: "MonitoredApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_MonitoredApps_AppId",
                        column: x => x.AppId,
                        principalTable: "MonitoredApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Id", "ActiveAiProvider", "AiApiKey", "AiApiUrl", "CriticalCpuThreshold", "MaxRamThresholdMb" },
                values: new object[] { 1, "Ollama", null, null, 90.0, 2048.0 });

            migrationBuilder.CreateIndex(
                name: "IX_AiInsights_AppId",
                table: "AiInsights",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthSnapshots_AppId",
                table: "HealthSnapshots",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AppId",
                table: "Incidents",
                column: "AppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiInsights");

            migrationBuilder.DropTable(
                name: "HealthSnapshots");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "MonitoredApps");
        }
    }
}
