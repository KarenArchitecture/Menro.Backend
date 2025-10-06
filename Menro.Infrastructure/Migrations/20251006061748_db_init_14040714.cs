using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class db_init_14040714 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodAddon_FoodVariant_FoodVariantId",
                table: "FoodAddon");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodVariant_Foods_FoodId",
                table: "FoodVariant");

            migrationBuilder.DropIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_FoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodVariant",
                table: "FoodVariant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodAddon",
                table: "FoodAddon");

            migrationBuilder.DropColumn(
                name: "FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "GlobalFoodCategoryId",
                table: "FoodCategories");

            migrationBuilder.RenameTable(
                name: "FoodVariant",
                newName: "FoodVariants");

            migrationBuilder.RenameTable(
                name: "FoodAddon",
                newName: "FoodAddons");

            migrationBuilder.RenameIndex(
                name: "IX_FoodVariant_FoodId",
                table: "FoodVariants",
                newName: "IX_FoodVariants_FoodId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodAddon_FoodVariantId",
                table: "FoodAddons",
                newName: "IX_FoodAddons_FoodVariantId");

            migrationBuilder.AddColumn<int>(
                name: "CustomFoodCategoryId",
                table: "Foods",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlobalFoodCategoryId",
                table: "Foods",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodVariants",
                table: "FoodVariants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodAddons",
                table: "FoodAddons",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_CustomFoodCategoryId",
                table: "Foods",
                column: "CustomFoodCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_GlobalFoodCategoryId",
                table: "Foods",
                column: "GlobalFoodCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodAddons_FoodVariants_FoodVariantId",
                table: "FoodAddons",
                column: "FoodVariantId",
                principalTable: "FoodVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_CustomFoodCategoryId",
                table: "Foods",
                column: "CustomFoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "Foods",
                column: "GlobalFoodCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodVariants_Foods_FoodId",
                table: "FoodVariants",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodAddons_FoodVariants_FoodVariantId",
                table: "FoodAddons");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodVariants_Foods_FoodId",
                table: "FoodVariants");

            migrationBuilder.DropIndex(
                name: "IX_Foods_CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Foods_GlobalFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodVariants",
                table: "FoodVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodAddons",
                table: "FoodAddons");

            migrationBuilder.DropColumn(
                name: "CustomFoodCategoryId",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "GlobalFoodCategoryId",
                table: "Foods");

            migrationBuilder.RenameTable(
                name: "FoodVariants",
                newName: "FoodVariant");

            migrationBuilder.RenameTable(
                name: "FoodAddons",
                newName: "FoodAddon");

            migrationBuilder.RenameIndex(
                name: "IX_FoodVariants_FoodId",
                table: "FoodVariant",
                newName: "IX_FoodVariant_FoodId");

            migrationBuilder.RenameIndex(
                name: "IX_FoodAddons_FoodVariantId",
                table: "FoodAddon",
                newName: "IX_FoodAddon_FoodVariantId");

            migrationBuilder.AddColumn<int>(
                name: "FoodCategoryId",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GlobalFoodCategoryId",
                table: "FoodCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodVariant",
                table: "FoodVariant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodAddon",
                table: "FoodAddon",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories",
                column: "GlobalFoodCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodAddon_FoodVariant_FoodVariantId",
                table: "FoodAddon",
                column: "FoodVariantId",
                principalTable: "FoodVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodCategories_GlobalFoodCategories_GlobalFoodCategoryId",
                table: "FoodCategories",
                column: "GlobalFoodCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodVariant_Foods_FoodId",
                table: "FoodVariant",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
