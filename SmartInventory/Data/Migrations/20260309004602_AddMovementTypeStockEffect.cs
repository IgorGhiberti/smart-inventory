using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventory.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementTypeStockEffect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockEffect",
                table: "MovementTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "StockEffect",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "StockEffect",
                value: -1);

            migrationBuilder.UpdateData(
                table: "MovementTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "StockEffect",
                value: -1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockEffect",
                table: "MovementTypes");
        }
    }
}
