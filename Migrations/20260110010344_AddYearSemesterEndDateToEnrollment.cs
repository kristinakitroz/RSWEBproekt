using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class AddYearSemesterEndDateToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Enrollments");
        }
    }
}
