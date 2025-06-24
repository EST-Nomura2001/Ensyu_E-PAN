using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreColumnToAllShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Store_ID",
                table: "All_Shifts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Store_ID",
                table: "All_Shifts");
        }
    }
}
