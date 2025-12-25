using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "UX_Exam_Student_Subject_Date",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Exam");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                columns: new[] { "Date", "SubjectID", "StudentID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Exam",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "UX_Exam_Student_Subject_Date",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "Date" },
                unique: true);
        }
    }
}
