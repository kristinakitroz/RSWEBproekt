using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsAndPassedOnToEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PassedOn",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Enrollments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassedOn",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Enrollments");
        }
    }
}
