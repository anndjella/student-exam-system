using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSchoolYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_SchoolYear_SchoolYearID",
                table: "Enrollment");

            migrationBuilder.DropTable(
                name: "SchoolYear");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Registration_Status",
                table: "Registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_SchoolYearID",
                table: "Enrollment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SchoolYearID",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollment");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Enrollment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "Enrollment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassedAt",
                table: "Enrollment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment",
                columns: new[] { "StudentID", "SubjectID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "PassedAt",
                table: "Enrollment");

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "Registration",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "SchoolYearID",
                table: "Enrollment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "Enrollment",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment",
                columns: new[] { "StudentID", "SubjectID", "SchoolYearID" });

            migrationBuilder.CreateTable(
                name: "SchoolYear",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolYear", x => x.ID);
                    table.CheckConstraint("CK_SchoolYear_EndDate_After_StartDate", "[EndDate]>[StartDate]");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Registration_Status",
                table: "Registration",
                sql: "[Status] BETWEEN 1 AND 2");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SchoolYearID",
                table: "Enrollment",
                column: "SchoolYearID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment",
                sql: "[Status] BETWEEN 1 AND 3");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_SchoolYear_SchoolYearID",
                table: "Enrollment",
                column: "SchoolYearID",
                principalTable: "SchoolYear",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
