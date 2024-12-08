using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PototoTrade.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserIdToSenderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "user_id8",
                table: "user_wallet",
                newName: "user_id7");

            migrationBuilder.RenameIndex(
                name: "user_id7",
                table: "user_session",
                newName: "user_id6");

            migrationBuilder.RenameIndex(
                name: "user_id6",
                table: "user_details",
                newName: "user_id5");

            migrationBuilder.RenameIndex(
                name: "user_id5",
                table: "user_activities_log",
                newName: "user_id4");

            migrationBuilder.RenameIndex(
                name: "user_id4",
                table: "shopping_cart",
                newName: "user_id3");

            migrationBuilder.RenameIndex(
                name: "user_id3",
                table: "purchase_order",
                newName: "user_id2");

            migrationBuilder.RenameIndex(
                name: "user_id2",
                table: "products",
                newName: "user_id1");

            migrationBuilder.RenameIndex(
                name: "user_id1",
                table: "product_review",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notification",
                newName: "sender_id");

            migrationBuilder.RenameIndex(
                name: "user_id",
                table: "notification",
                newName: "sender_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "received_at",
                table: "user_notification",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "user_id7",
                table: "user_wallet",
                newName: "user_id8");

            migrationBuilder.RenameIndex(
                name: "user_id6",
                table: "user_session",
                newName: "user_id7");

            migrationBuilder.RenameIndex(
                name: "user_id5",
                table: "user_details",
                newName: "user_id6");

            migrationBuilder.RenameIndex(
                name: "user_id4",
                table: "user_activities_log",
                newName: "user_id5");

            migrationBuilder.RenameIndex(
                name: "user_id3",
                table: "shopping_cart",
                newName: "user_id4");

            migrationBuilder.RenameIndex(
                name: "user_id2",
                table: "purchase_order",
                newName: "user_id3");

            migrationBuilder.RenameIndex(
                name: "user_id1",
                table: "products",
                newName: "user_id2");

            migrationBuilder.RenameIndex(
                name: "user_id",
                table: "product_review",
                newName: "user_id1");

            migrationBuilder.RenameColumn(
                name: "sender_id",
                table: "notification",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "sender_id",
                table: "notification",
                newName: "user_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "received_at",
                table: "user_notification",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }
    }
}
