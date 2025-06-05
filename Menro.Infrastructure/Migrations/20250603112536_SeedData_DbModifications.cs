using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData_DbModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodFoodCategory");

            migrationBuilder.DropTable(
                name: "RestaurantRestaurantCategory");

            migrationBuilder.DropColumn(
                name: "OwnerFirstName",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "OwnerLastName",
                table: "Restaurants");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Restaurants",
                newName: "OwnerFullName");

            migrationBuilder.AlterColumn<string>(
                name: "ShebaNumber",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BannerImageUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantCategoryId",
                table: "Restaurants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FoodCategoryId",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "FoodCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "RestaurantCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "رستوران سنتی" },
                    { 2, "رستوران مدرن" },
                    { 3, "چینی/آسیایی" },
                    { 4, "ایتالیایی" },
                    { 5, "کافه رستوران" },
                    { 6, "فست‌فود" },
                    { 7, "باغ رستوران" },
                    { 8, "دریایی" }
                });

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "Address", "BankAccountNumber", "BannerImageUrl", "Name", "NationalCode", "OwnerFullName", "PhoneNumber", "RestaurantCategoryId", "ShebaNumber" },
                values: new object[] { 1, "خیابان ولیعصر، پلاک ۱۲۳", "1234567890", null, "کافه منرو", "0012345678", "امین منرو", "09123456789", 5, "IR820540102680020817909002" });

            migrationBuilder.InsertData(
                table: "FoodCategories",
                columns: new[] { "Id", "Name", "RestaurantId" },
                values: new object[,]
                {
                    { 1, "نوشیدنی سرد", 1 },
                    { 2, "نوشیدنی گرم", 1 },
                    { 3, "پیتزا", 1 },
                    { 4, "پاستا", 1 },
                    { 5, "سالاد", 1 },
                    { 6, "دسر", 1 },
                    { 7, "سوپ", 1 },
                    { 8, "برگر", 1 },
                    { 9, "غذای دریایی", 1 },
                    { 10, "پیش‌غذا", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_RestaurantCategoryId",
                table: "Restaurants",
                column: "RestaurantCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_RestaurantId",
                table: "FoodCategories",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_Restaurants_RestaurantId",
                table: "FoodCategories",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_RestaurantCategories_RestaurantCategoryId",
                table: "Restaurants",
                column: "RestaurantCategoryId",
                principalTable: "RestaurantCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_Restaurants_RestaurantId",
                table: "FoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_RestaurantCategories_RestaurantCategoryId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_RestaurantCategoryId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_RestaurantId",
                table: "FoodCategories");

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "FoodCategories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RestaurantCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "RestaurantCategoryId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "FoodCategories");

            migrationBuilder.RenameColumn(
                name: "OwnerFullName",
                table: "Restaurants",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "ShebaNumber",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BannerImageUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerFirstName",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerLastName",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FoodFoodCategory",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    FoodsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodFoodCategory", x => new { x.CategoriesId, x.FoodsId });
                    table.ForeignKey(
                        name: "FK_FoodFoodCategory_FoodCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "FoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodFoodCategory_Foods_FoodsId",
                        column: x => x.FoodsId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantRestaurantCategory",
                columns: table => new
                {
                    RestaurantCategoriesId = table.Column<int>(type: "int", nullable: false),
                    RestaurantsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantRestaurantCategory", x => new { x.RestaurantCategoriesId, x.RestaurantsId });
                    table.ForeignKey(
                        name: "FK_RestaurantRestaurantCategory_RestaurantCategories_RestaurantCategoriesId",
                        column: x => x.RestaurantCategoriesId,
                        principalTable: "RestaurantCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestaurantRestaurantCategory_Restaurants_RestaurantsId",
                        column: x => x.RestaurantsId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodFoodCategory_FoodsId",
                table: "FoodFoodCategory",
                column: "FoodsId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantRestaurantCategory_RestaurantsId",
                table: "RestaurantRestaurantCategory",
                column: "RestaurantsId");
        }
    }
}
