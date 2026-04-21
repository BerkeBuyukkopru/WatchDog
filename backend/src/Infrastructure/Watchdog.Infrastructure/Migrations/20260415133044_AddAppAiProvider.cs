using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppAiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveAiProviderId",
                table: "MonitoredApps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredApps_ActiveAiProviderId",
                table: "MonitoredApps",
                column: "ActiveAiProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredApps_AiProviders_ActiveAiProviderId",
                table: "MonitoredApps",
                column: "ActiveAiProviderId",
                principalTable: "AiProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredApps_AiProviders_ActiveAiProviderId",
                table: "MonitoredApps");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredApps_ActiveAiProviderId",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "ActiveAiProviderId",
                table: "MonitoredApps");
        }
    }
}
