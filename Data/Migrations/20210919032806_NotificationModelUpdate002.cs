using Microsoft.EntityFrameworkCore.Migrations;

namespace TitanTracker.Data.Migrations
{
    public partial class NotificationModelUpdate002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archived",
                table: "Notifications");
        }
    }
}
