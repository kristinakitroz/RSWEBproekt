using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace RSWEBproekt.Migrations
{
    /// <inheritdoc />
    public partial class InitalTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AcademicRank = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    OfficeNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
//EF Core go napraj slednovo:
//Gi procita modelite (Teacher)
//Go procita ApplicationDbContext
//Napravi snapshot (slika) na momentalnata sostojba na modelite
//Generira C# kod sto opisuva:
//„Kako treba da izgleda bazata na podatoci“
//Add - Migration - pravenje plan
//Update-Database - izvrsuvanje na planot