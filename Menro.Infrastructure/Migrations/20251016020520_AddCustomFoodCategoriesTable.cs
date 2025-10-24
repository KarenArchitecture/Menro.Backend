using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFoodCategoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_Restaurants_RestaurantId",
                table: "FoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodCategories",
                table: "FoodCategories");

            migrationBuilder.RenameTable(
                name: "FoodCategories",
                newName: "CustomFoodCategories");

            migrationBuilder.RenameIndex(
                name: "IX_FoodCategories_RestaurantId_Name",
                table: "CustomFoodCategories",
                newName: "IX_CustomFoodCategories_RestaurantId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomFoodCategories",
                table: "CustomFoodCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategories_Restaurants_RestaurantId",
                table: "CustomFoodCategories",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_CustomFoodCategories_CustomFoodCategoryId",
                table: "Foods",
                column: "CustomFoodCategoryId",
                principalTable: "CustomFoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategories_Restaurants_RestaurantId",
                table: "CustomFoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_CustomFoodCategories_CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomFoodCategories",
                table: "CustomFoodCategories");

            migrationBuilder.RenameTable(
                name: "CustomFoodCategories",
                newName: "FoodCategories");

            migrationBuilder.RenameIndex(
                name: "IX_CustomFoodCategories_RestaurantId_Name",
                table: "FoodCategories",
                newName: "IX_FoodCategories_RestaurantId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodCategories",
                table: "FoodCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_Restaurants_RestaurantId",
                table: "FoodCategories",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_CustomFoodCategoryId",
                table: "Foods",
                column: "CustomFoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
