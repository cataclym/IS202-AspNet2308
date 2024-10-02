using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kartverket.Migrations
{
    /// <inheritdoc />
    public partial class fk_new_logindata_feilmelding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoginDataUserId",
                table: "geo_data",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_geo_data_LoginDataUserId",
                table: "geo_data",
                column: "LoginDataUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_geo_data_LoginData_LoginDataUserId",
                table: "geo_data",
                column: "LoginDataUserId",
                principalTable: "LoginData",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_geo_data_LoginData_LoginDataUserId",
                table: "geo_data");

            migrationBuilder.DropIndex(
                name: "IX_geo_data_LoginDataUserId",
                table: "geo_data");

            migrationBuilder.DropColumn(
                name: "LoginDataUserId",
                table: "geo_data");
        }
    }
}
