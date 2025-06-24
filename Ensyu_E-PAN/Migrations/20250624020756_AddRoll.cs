using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class AddRoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Roll_Lists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Roll_Lists");
        }
    }
}
