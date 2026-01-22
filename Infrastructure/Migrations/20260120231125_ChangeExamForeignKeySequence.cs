using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeExamForeignKeySequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Registration_StudentID_SubjectID_TermID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_StudentID_SubjectID_TermID",
                table: "Exam");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SubjectID_StudentID_TermID",
                table: "Exam",
                columns: new[] { "SubjectID", "StudentID", "TermID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Registration_SubjectID_StudentID_TermID",
                table: "Exam",
                columns: new[] { "SubjectID", "StudentID", "TermID" },
                principalTable: "Registration",
                principalColumns: new[] { "SubjectID", "StudentID", "TermID" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Registration_SubjectID_StudentID_TermID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_SubjectID_StudentID_TermID",
                table: "Exam");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_StudentID_SubjectID_TermID",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "TermID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Registration_StudentID_SubjectID_TermID",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "TermID" },
                principalTable: "Registration",
                principalColumns: new[] { "SubjectID", "StudentID", "TermID" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
