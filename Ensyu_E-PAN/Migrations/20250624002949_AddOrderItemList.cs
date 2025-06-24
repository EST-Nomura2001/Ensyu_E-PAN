using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ensyu_E_PAN.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Other_ItemName",
                table: "Order_Item_Lists",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Other_ItemName",
                table: "Order_Item_Lists");
        }
    }
}
