using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class EditUserShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Shifts_Day_Shifts_Day_Shifts_Id",
                table: "User_Shifts");

            migrationBuilder.DropIndex(
                name: "IX_User_Shifts_Day_Shifts_Id",
                table: "User_Shifts");

            migrationBuilder.DropColumn(
                name: "Day_Shifts_Id",
                table: "User_Shifts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Day_Shifts_Id",
                table: "User_Shifts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_Shifts_Day_Shifts_Id",
                table: "User_Shifts",
                column: "Day_Shifts_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Shifts_Day_Shifts_Day_Shifts_Id",
                table: "User_Shifts",
                column: "Day_Shifts_Id",
                principalTable: "Day_Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
