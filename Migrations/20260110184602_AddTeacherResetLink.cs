using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherResetLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingResetLink",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetLinkCreatedOn",
                table: "Teachers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingResetLink",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ResetLinkCreatedOn",
                table: "Teachers");
        }
    }
}
