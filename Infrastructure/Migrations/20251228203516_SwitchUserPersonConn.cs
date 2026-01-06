using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwitchUserPersonConn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_User_UserID",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_UserID",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Person");

            migrationBuilder.AddColumn<int>(
                name: "PersonID",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_PersonID",
                table: "User",
                column: "PersonID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Person_PersonID",
                table: "User",
                column: "PersonID",
                principalTable: "Person",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Person_PersonID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PersonID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PersonID",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Person",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Person_UserID",
                table: "Person",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_User_UserID",
                table: "Person",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
