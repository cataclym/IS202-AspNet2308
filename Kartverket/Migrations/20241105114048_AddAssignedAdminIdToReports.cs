using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kartverket.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedAdminIdToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedAdminId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignedAdminId",
                table: "Reports",
                column: "AssignedAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports",
                column: "AssignedAdminId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignedAdminId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "AssignedAdminId",
                table: "Reports");
        }
    }
}
