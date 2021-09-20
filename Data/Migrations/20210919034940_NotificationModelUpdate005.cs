using Microsoft.EntityFrameworkCore.Migrations;

namespace TitanTracker.Data.Migrations
{
    public partial class NotificationModelUpdate005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Important",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Important",
                table: "Notifications");
        }
    }
}
