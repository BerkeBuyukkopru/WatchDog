using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchdog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationEmailsToApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationEmails",
                table: "MonitoredApps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationEmails",
                table: "MonitoredApps");
        }
    }
}
