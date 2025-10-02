using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_WithTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JMBG = table.Column<string>(type: "char(13)", unicode: false, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false, computedColumnSql: "DATEDIFF(YEAR, [DateOfBirth], GETDATE()) - CASE WHEN FORMAT(GETDATE(),'MMdd') < FORMAT([DateOfBirth],'MMdd') THEN 1 ELSE 0 END", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.ID);
                    table.CheckConstraint("CK_Person_JMBG_13Digits", "LEN([JMBG]) = 13 AND PATINDEX('%[^0-9]%', [JMBG]) = 0");
                });

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ESPB = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.ID);
                    table.CheckConstraint("CK_Subject_Espb", "[ESPB] BETWEEN 1 AND 60");
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    IndexNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Student_Person_ID",
                        column: x => x.ID,
                        principalTable: "Person",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teacher",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher", x => x.ID);
                    table.CheckConstraint("CK_Teacher_Title", "[Title] BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "FK_Teacher_Person_ID",
                        column: x => x.ID,
                        principalTable: "Person",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exam",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<byte>(type: "tinyint", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    ExaminerID = table.Column<int>(type: "int", nullable: false),
                    SupervisorID = table.Column<int>(type: "int", nullable: true),
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam", x => x.ID);
                    table.CheckConstraint("CK_Exam_Grade", "[Grade] BETWEEN 5 AND 10");
                    table.ForeignKey(
                        name: "FK_Exam_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exam_Subject_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exam_Teacher_ExaminerID",
                        column: x => x.ExaminerID,
                        principalTable: "Teacher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exam_Teacher_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Teacher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exam_ExaminerID",
                table: "Exam",
                column: "ExaminerID");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_StudentID",
                table: "Exam",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SubjectID",
                table: "Exam",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SupervisorID",
                table: "Exam",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_Person_JMBG",
                table: "Person",
                column: "JMBG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_IndexNumber",
                table: "Student",
                column: "IndexNumber",
                unique: true,
                filter: "[IndexNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exam");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "Teacher");

            migrationBuilder.DropTable(
                name: "Person");
        }
    }
}
