using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfBirthRangeCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person",
                sql: "[DateOfBirth] >= '1900-01-01' AND [DateOfBirth] <= '2008-12-31'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Person_DateOfBirth_Range",
                table: "Person");
        }
    }
}
