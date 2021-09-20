using Microsoft.EntityFrameworkCore.Migrations;

namespace TitanTracker.Data.Migrations
{
    public partial class NotificationModelDraftAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Draft",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Draft",
                table: "Notifications");
        }
    }
}
