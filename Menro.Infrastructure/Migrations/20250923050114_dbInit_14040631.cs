using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dbInit_14040631 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodAddon_Foods_FoodId",
                table: "FoodAddon");

            migrationBuilder.RenameColumn(
                name: "FoodId",
                table: "FoodAddon",
                newName: "FoodVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodAddon_FoodId",
                table: "FoodAddon",
                newName: "IX_FoodAddon_FoodVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodAddon_FoodVariant_FoodVariantId",
                table: "FoodAddon",
                column: "FoodVariantId",
                principalTable: "FoodVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodAddon_FoodVariant_FoodVariantId",
                table: "FoodAddon");

            migrationBuilder.RenameColumn(
                name: "FoodVariantId",
                table: "FoodAddon",
                newName: "FoodId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodAddon_FoodVariantId",
                table: "FoodAddon",
                newName: "IX_FoodAddon_FoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodAddon_Foods_FoodId",
                table: "FoodAddon",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
