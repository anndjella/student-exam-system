using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewStudentStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR ALTER VIEW dbo.vw_StudentStats
            AS
            SELECT
                st.ID AS StudentID,
                CAST(
                    AVG(
                        CAST(
                            CASE 
                                WHEN e.SignedAt IS NOT NULL AND e.Grade >= 6 
                                THEN e.Grade 
                            END 
                        AS decimal(10,2))
                    )
                AS decimal(4,2)) AS GPA,
                COALESCE(
                    SUM(
                        CASE 
                            WHEN e.SignedAt IS NOT NULL AND e.Grade >= 6 
                            THEN s.ECTS 
                            ELSE 0 
                        END
                    ),
                    0
                ) AS ECTSCount
            FROM dbo.Student st
            LEFT JOIN dbo.Exam e
                ON e.StudentID = st.ID
            LEFT JOIN dbo.Subject s
                ON s.ID = e.SubjectID
            GROUP BY st.ID;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS dbo.vw_StudentStats;");
        }
    }
}
