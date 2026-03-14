using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Student_StudentID",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Subject_SubjectID",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Teacher_ExaminerID",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Teacher_SupervisorID",
                table: "Exam");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Subject_Espb",
                table: "Subject");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Person_JMBG_DateOfBirth",
                table: "Person");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_ExaminerID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_SubjectID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_SupervisorID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "UX_Exam_PassOnce",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "GPA",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "SupervisorID",
                table: "Exam");

            migrationBuilder.RenameColumn(
                name: "ESPB",
                table: "Subject",
                newName: "ECTS");

            migrationBuilder.RenameColumn(
                name: "ExaminerID",
                table: "Exam",
                newName: "TermID");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                table: "Teacher",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "IndexNumber",
                table: "Student",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Person",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<byte>(
                name: "Grade",
                table: "Exam",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Exam",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAt",
                table: "Exam",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherID",
                table: "Exam",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "SchoolYear",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolYear", x => x.ID);
                    table.CheckConstraint("CK_SchoolYear_EndDate_After_StartDate", "[EndDate]>[StartDate]");
                });

            migrationBuilder.CreateTable(
                name: "TeachingAssignment",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    CanGrade = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingAssignment", x => new { x.SubjectID, x.TeacherID });
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Subject_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Teacher_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teacher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Term",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistrationStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistrationEndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Term", x => x.ID);
                    table.CheckConstraint("CK_Term_EndDate_After_StartDate", "[EndDate] > [StartDate]");
                    table.CheckConstraint("CK_Term_RegEndDate_After_RegStartDate", "[RegistrationEndDate] > [RegistrationStartDate]");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ID);
                    table.CheckConstraint("CK_User_Role", "[Role] BETWEEN 1 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    SchoolYearID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => new { x.StudentID, x.SubjectID, x.SchoolYearID });
                    table.CheckConstraint("CK_Enrollment_Status", "[Status] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_Enrollment_SchoolYear_SchoolYearID",
                        column: x => x.SchoolYearID,
                        principalTable: "SchoolYear",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_Subject_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration", x => new { x.SubjectID, x.StudentID, x.TermID });
                    table.CheckConstraint("CK_Registration_Status", "[Status] BETWEEN 1 AND 2");
                    table.ForeignKey(
                        name: "FK_Registration_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registration_Subject_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registration_Term_TermID",
                        column: x => x.TermID,
                        principalTable: "Term",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_EmployeeNumber",
                table: "Teacher",
                column: "EmployeeNumber",
                unique: true,
                filter: "[EmployeeNumber] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Teacher_EmployeeNumber_Format",
                table: "Teacher",
                sql: "[EmployeeNumber] LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Subject_ECTS",
                table: "Subject",
                sql: "[ECTS] BETWEEN 1 AND 15");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student",
                sql: "[IndexNumber] LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]'");

            migrationBuilder.CreateIndex(
                name: "IX_Person_UserID",
                table: "Person",
                column: "UserID",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person",
                sql: "[DateOfBirth] >= '1900-01-01' AND [DateOfBirth] <= CONVERT(date, GETDATE())");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_StudentID_SubjectID_TermID",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "TermID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exam_TeacherID",
                table: "Exam",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SchoolYearID",
                table: "Enrollment",
                column: "SchoolYearID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SubjectID",
                table: "Enrollment",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_StudentID",
                table: "Registration",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_TermID",
                table: "Registration",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssignment_TeacherID",
                table: "TeachingAssignment",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Registration_StudentID_SubjectID_TermID",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID", "TermID" },
                principalTable: "Registration",
                principalColumns: new[] { "SubjectID", "StudentID", "TermID" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Teacher_TeacherID",
                table: "Exam",
                column: "TeacherID",
                principalTable: "Teacher",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_User_UserID",
                table: "Person",
                column: "UserID",
                principalTable: "User",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Registration_StudentID_SubjectID_TermID",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Teacher_TeacherID",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_User_UserID",
                table: "Person");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Registration");

            migrationBuilder.DropTable(
                name: "TeachingAssignment");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SchoolYear");

            migrationBuilder.DropTable(
                name: "Term");

            migrationBuilder.DropIndex(
                name: "IX_Teacher_EmployeeNumber",
                table: "Teacher");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Teacher_EmployeeNumber_Format",
                table: "Teacher");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Subject_ECTS",
                table: "Subject");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_Person_UserID",
                table: "Person");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_StudentID_SubjectID_TermID",
                table: "Exam");

            migrationBuilder.DropIndex(
                name: "IX_Exam_TeacherID",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                table: "Teacher");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "Exam");

            migrationBuilder.RenameColumn(
                name: "ECTS",
                table: "Subject",
                newName: "ESPB");

            migrationBuilder.RenameColumn(
                name: "TermID",
                table: "Exam",
                newName: "ExaminerID");

            migrationBuilder.AlterColumn<string>(
                name: "IndexNumber",
                table: "Student",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AddColumn<decimal>(
                name: "GPA",
                table: "Student",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Grade",
                table: "Exam",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorID",
                table: "Exam",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Person",
                type: "int",
                nullable: false,
                computedColumnSql: "DATEDIFF(YEAR, [DateOfBirth], GETDATE()) - CASE WHEN FORMAT(GETDATE(),'MMdd') < FORMAT([DateOfBirth],'MMdd') THEN 1 ELSE 0 END",
                stored: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                columns: new[] { "Date", "SubjectID", "StudentID" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Subject_Espb",
                table: "Subject",
                sql: "[ESPB] BETWEEN 1 AND 60");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student",
                sql: "[IndexNumber] NOT LIKE '%[^0-9/]%' AND CHARINDEX('/', [IndexNumber]) = 5 AND LEN([IndexNumber]) BETWEEN 6 AND 10 AND RIGHT([IndexNumber], LEN([IndexNumber]) - 5) NOT LIKE '%[^0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person",
                sql: "[DateOfBirth] >= '1900-01-01' AND [DateOfBirth] <= '2008-12-31'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Person_JMBG_DateOfBirth",
                table: "Person",
                sql: "LEFT([JMBG], 7) =     RIGHT('00' + CAST(DATEPART(DAY, [DateOfBirth]) AS varchar(2)), 2) +    RIGHT('00' + CAST(DATEPART(MONTH, [DateOfBirth]) AS varchar(2)), 2) +    RIGHT('000' + CAST(DATEPART(YEAR, [DateOfBirth]) AS varchar(4)), 3)");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_ExaminerID",
                table: "Exam",
                column: "ExaminerID");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SubjectID",
                table: "Exam",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SupervisorID",
                table: "Exam",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "UX_Exam_PassOnce",
                table: "Exam",
                columns: new[] { "StudentID", "SubjectID" },
                unique: true,
                filter: "[Grade] >= 6");

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Student_StudentID",
                table: "Exam",
                column: "StudentID",
                principalTable: "Student",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Subject_SubjectID",
                table: "Exam",
                column: "SubjectID",
                principalTable: "Subject",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Teacher_ExaminerID",
                table: "Exam",
                column: "ExaminerID",
                principalTable: "Teacher",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Teacher_SupervisorID",
                table: "Exam",
                column: "SupervisorID",
                principalTable: "Teacher",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
