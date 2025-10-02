using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGpaColumn_And_Trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GPA", table: "Student",
                type: "decimal(4,2)", nullable: true);

            // 1 poziv (ako server podržava): 
            migrationBuilder.Sql(@"
                CREATE OR ALTER TRIGGER dbo.trg_Exam_UpdateGpa
                ON dbo.Exam
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                  SET NOCOUNT ON;
                  ;WITH affected AS (
                    SELECT DISTINCT StudentID FROM inserted WHERE StudentID IS NOT NULL
                    UNION
                    SELECT DISTINCT StudentID FROM deleted  WHERE StudentID IS NOT NULL
                  ),
                  agg AS (
                    SELECT e.StudentID,
                           CAST(AVG(CASE WHEN e.Grade >= 6 
                               THEN CAST(e.Grade AS DECIMAL(4,2)) END) AS DECIMAL(4,2)) AS Average
                    FROM dbo.Exam e
                    JOIN affected a ON a.StudentID = e.StudentID
                    GROUP BY e.StudentID
                  )
                  UPDATE s SET s.GPA = a.Average
                  FROM dbo.Student s JOIN agg a ON a.StudentID = s.ID;
                
                  UPDATE s SET s.GPA = NULL
                  FROM dbo.Student s
                  WHERE s.ID IN (SELECT DISTINCT StudentID FROM inserted WHERE StudentID IS NOT NULL
                                 UNION SELECT DISTINCT StudentID FROM deleted  WHERE StudentID IS NOT NULL)
                    AND NOT EXISTS (SELECT 1 FROM dbo.Exam e WHERE e.StudentID = s.ID AND e.Grade >= 6);
                END
                ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.trg_Exam_UpdateGpa','TR') IS NOT NULL DROP TRIGGER dbo.trg_Exam_UpdateGpa;");
            migrationBuilder.DropColumn(name: "GPA", table: "Student");
        }
    }
}
