using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Person_JMBG",
                table: "Person");

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Subject",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Subject",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Person",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Person",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Person_JMBG",
                table: "Person",
                column: "JMBG",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Person_JMBG",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "User");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Person");

            migrationBuilder.CreateIndex(
                name: "IX_Person_JMBG",
                table: "Person",
                column: "JMBG",
                unique: true);
        }
    }
}
