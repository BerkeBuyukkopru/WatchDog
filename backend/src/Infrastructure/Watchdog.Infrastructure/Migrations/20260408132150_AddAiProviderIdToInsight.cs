using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProviderIdToInsight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AiProviderId",
                table: "AiInsights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiInsights_AiProviderId",
                table: "AiInsights",
                column: "AiProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiInsights_AiProviders_AiProviderId",
                table: "AiInsights",
                column: "AiProviderId",
                principalTable: "AiProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiInsights_AiProviders_AiProviderId",
                table: "AiInsights");

            migrationBuilder.DropIndex(
                name: "IX_AiInsights_AiProviderId",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "AiProviderId",
                table: "AiInsights");
        }
    }
}
