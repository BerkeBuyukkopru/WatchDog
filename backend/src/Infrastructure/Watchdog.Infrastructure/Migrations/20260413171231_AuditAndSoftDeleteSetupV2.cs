using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditAndSoftDeleteSetupV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SystemConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MonitoredApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MonitoredApps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "MonitoredApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MonitoredApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "MonitoredApps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MonitoredApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Incidents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HealthSnapshots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HealthSnapshots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HealthSnapshots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HealthSnapshots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HealthSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "HealthSnapshots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "HealthSnapshots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AiProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "AiProviders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AiInsights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AiInsights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AiInsights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AiInsights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "AiInsights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "AiInsights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AdminUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AdminUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AdminUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AdminUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "AdminUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "AdminUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy" },
                values: new object[] { "System", null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy" },
                values: new object[] { "System", null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy" },
                values: new object[] { "System", null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", null, null, false, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MonitoredApps");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "HealthSnapshots");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "AiInsights");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "AdminUsers");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "AdminUsers");
        }
    }
}
