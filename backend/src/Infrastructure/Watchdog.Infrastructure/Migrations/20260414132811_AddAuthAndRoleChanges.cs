using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndRoleChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AiProviders",
                columns: new[] { "Id", "ApiKey", "ApiUrl", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsActive", "IsDeleted", "ModelName", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, "http://localhost:11434", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, true, false, "phi3:medium", null, null, "Ollama" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, false, false, "gpt-4o-mini", null, null, "OpenAI" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "https://api.groq.com/openai/v1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, false, false, "llama-3.3-70b-versatile", null, null, "Groq" }
                });

            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CriticalCpuThreshold", "CriticalLatencyThreshold", "CriticalRamThreshold", "DeletedAt", "DeletedBy", "IsDeleted", "LastArchivedDate", "LastUpdated", "ModifiedAt", "ModifiedBy" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", 90.0, 1000.0, 90.0, null, null, false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null });
        }
    }
}
