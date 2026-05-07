using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "LastUpdated",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "MonitoredApps");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "LastUpdated",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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
                name: "ApiKey",
                table: "MonitoredApps",
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
        }
    }
}
