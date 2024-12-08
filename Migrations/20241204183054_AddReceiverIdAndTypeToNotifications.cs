using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PototoTrade.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiverIdAndTypeToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverId",
                table: "notification",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
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
                name: "ReceiverId",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "notification");
        }
    }
}
