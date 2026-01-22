using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Subject");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Subject",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOffered",
                table: "Subject",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subject_Code",
                table: "Subject",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subject_Code",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "IsOffered",
                table: "Subject");

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
        }
    }
}
