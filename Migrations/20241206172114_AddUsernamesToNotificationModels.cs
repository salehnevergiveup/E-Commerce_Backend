using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PototoTrade.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernamesToNotificationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserUsername",
                table: "user_notification",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUsername",
                table: "notification",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SenderUsername",
                table: "notification",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserUsername",
                table: "user_notification");

            migrationBuilder.DropColumn(
                name: "ReceiverUsername",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "SenderUsername",
                table: "notification");
        }
    }
}
