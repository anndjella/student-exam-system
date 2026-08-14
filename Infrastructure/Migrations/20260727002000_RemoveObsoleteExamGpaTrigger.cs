using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260727002000_RemoveObsoleteExamGpaTrigger")]
public sealed class RemoveObsoleteExamGpaTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_Exam_UpdateGpa;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The trigger cannot be restored because Student.GPA no longer exists.
        // GPA and ECTS statistics are owned by dbo.vw_StudentStats.
    }
}
