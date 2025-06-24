using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_All_Shifts_Store_ID",
                table: "All_Shifts",
                column: "Store_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_All_Shifts_Stores_Store_ID",
                table: "All_Shifts",
                column: "Store_ID",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_All_Shifts_Stores_Store_ID",
                table: "All_Shifts");

            migrationBuilder.DropIndex(
                name: "IX_All_Shifts_Store_ID",
                table: "All_Shifts");
        }
    }
}
