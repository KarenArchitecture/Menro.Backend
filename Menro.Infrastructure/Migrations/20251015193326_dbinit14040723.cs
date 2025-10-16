using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dbinit14040723 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlobalCategoryId",
                table: "FoodCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_GlobalCategoryId",
                table: "FoodCategories",
                column: "GlobalCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "FoodCategories",
                column: "GlobalCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_GlobalCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "GlobalCategoryId",
                table: "FoodCategories");
        }
    }
}
