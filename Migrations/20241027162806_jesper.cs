using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kartverket.Migrations
{
    /// <inheritdoc />
    public partial class jesper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UsersUserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UsersUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UsersUserId",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "UsersUserId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UsersUserId",
                table: "Reports",
                column: "UsersUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UsersUserId",
                table: "Reports",
                column: "UsersUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
