using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTeachers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Courses_FirstTeacherId",
                table: "Courses",
                column: "FirstTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SecondTeacherId",
                table: "Courses",
                column: "SecondTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_FirstTeacherId",
                table: "Courses",
                column: "FirstTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_SecondTeacherId",
                table: "Courses",
                column: "SecondTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_FirstTeacherId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_SecondTeacherId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_FirstTeacherId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SecondTeacherId",
                table: "Courses");
        }
    }
}
