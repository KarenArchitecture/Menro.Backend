using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dbinit14040723_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "FoodCategories");

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
                newName: "CustomFoodCategory");

            migrationBuilder.RenameIndex(
                name: "IX_FoodCategories_RestaurantId_Name",
                table: "CustomFoodCategory",
                newName: "IX_CustomFoodCategory_RestaurantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_FoodCategories_GlobalCategoryId",
                table: "CustomFoodCategory",
                newName: "IX_CustomFoodCategory_GlobalCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomFoodCategory",
                table: "CustomFoodCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategory_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategory",
                column: "GlobalCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategory_Restaurants_RestaurantId",
                table: "CustomFoodCategory",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_CustomFoodCategory_CustomFoodCategoryId",
                table: "Foods",
                column: "CustomFoodCategoryId",
                principalTable: "CustomFoodCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategory_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategory_Restaurants_RestaurantId",
                table: "CustomFoodCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_CustomFoodCategory_CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomFoodCategory",
                table: "CustomFoodCategory");

            migrationBuilder.RenameTable(
                name: "CustomFoodCategory",
                newName: "FoodCategories");

            migrationBuilder.RenameIndex(
                name: "IX_CustomFoodCategory_RestaurantId_Name",
                table: "FoodCategories",
                newName: "IX_FoodCategories_RestaurantId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CustomFoodCategory_GlobalCategoryId",
                table: "FoodCategories",
                newName: "IX_FoodCategories_GlobalCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodCategories",
                table: "FoodCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "FoodCategories",
                column: "GlobalCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id");

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
