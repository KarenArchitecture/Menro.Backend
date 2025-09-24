using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABGlobalFoodCategoryAndNewSeedingAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_RestaurantId",
                table: "FoodCategories");

            migrationBuilder.AddColumn<int>(
                name: "GlobalFoodCategoryId",
                table: "FoodCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlobalFoodCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SvgIcon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalFoodCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories",
                column: "GlobalFoodCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_RestaurantId_Name",
                table: "FoodCategories",
                columns: new[] { "RestaurantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalFoodCategories_Name",
                table: "GlobalFoodCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories",
                column: "GlobalFoodCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropTable(
                name: "GlobalFoodCategories");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_RestaurantId_Name",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_RestaurantId",
                table: "FoodCategories",
                column: "RestaurantId");
        }
    }
}
