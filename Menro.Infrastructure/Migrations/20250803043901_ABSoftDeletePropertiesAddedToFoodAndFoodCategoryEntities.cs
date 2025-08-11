using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABSoftDeletePropertiesAddedToFoodAndFoodCategoryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Foods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Foods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "FoodCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FoodCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FoodCategories");
        }
    }
}
