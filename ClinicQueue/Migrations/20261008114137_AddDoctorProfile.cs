using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicQueue.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Doctors",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubSpecialties",
                table: "Doctors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SubSpecialties",
                table: "Doctors");
        }
    }
}
