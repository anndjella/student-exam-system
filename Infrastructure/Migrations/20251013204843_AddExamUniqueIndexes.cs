using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exam_StudentID",
                table: "Exam");

            migrationBuilder.CreateIndex(
                name: "UX_Exam_PassOnce",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID" },
                unique: true,
                filter: "[Grade] >= 6");

            migrationBuilder.CreateIndex(
                name: "UX_Exam_Student_Subject_Date",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Exam_PassOnce",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "UX_Exam_Student_Subject_Date",
                table: "Exam");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_StudentID",
                table: "Exam",
                column: "StudentID");
        }
    }
}
