using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectUrlEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectUrl",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectUrl",
                table: "Enrollments");
        }
    }
}
