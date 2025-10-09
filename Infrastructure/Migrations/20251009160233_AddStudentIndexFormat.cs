using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIndexFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student",
                sql: "[IndexNumber] NOT LIKE '%[^0-9/]%' AND CHARINDEX('/', [IndexNumber]) = 5 AND LEN([IndexNumber]) BETWEEN 6 AND 10 AND RIGHT([IndexNumber], LEN([IndexNumber]) - 5) NOT LIKE '%[^0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Person_JMBG_DateOfBirth",
                table: "Person",
                sql: "LEFT([JMBG], 7) =     RIGHT('00' + CAST(DATEPART(DAY, [DateOfBirth]) AS varchar(2)), 2) +    RIGHT('00' + CAST(DATEPART(MONTH, [DateOfBirth]) AS varchar(2)), 2) +    RIGHT('000' + CAST(DATEPART(YEAR, [DateOfBirth]) AS varchar(4)), 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Student_IndexNumber_Format",
                table: "Student");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Person_JMBG_DateOfBirth",
                table: "Person");
        }
    }
}
