using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicQueue.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyCapacity",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyCapacity",
                table: "Doctors");
        }
    }
}
