using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kartverket.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedAdminRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports",
                column: "AssignedAdminId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_AssignedAdminId",
                table: "Reports",
                column: "AssignedAdminId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
