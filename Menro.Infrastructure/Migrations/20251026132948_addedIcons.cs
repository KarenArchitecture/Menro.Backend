using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategories");

            migrationBuilder.DropColumn(
                name: "SvgIcon",
                table: "GlobalFoodCategories");

            migrationBuilder.DropColumn(
                name: "SvgIcon",
                table: "CustomFoodCategories");

            migrationBuilder.AddColumn<int>(
                name: "IconId",
                table: "GlobalFoodCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IconId",
                table: "CustomFoodCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Icons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalFoodCategories_IconId",
                table: "GlobalFoodCategories",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFoodCategories_IconId",
                table: "CustomFoodCategories",
                column: "IconId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategories",
                column: "GlobalCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategories_Icons_IconId",
                table: "CustomFoodCategories",
                column: "IconId",
                principalTable: "Icons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalFoodCategories_Icons_IconId",
                table: "GlobalFoodCategories",
                column: "IconId",
                principalTable: "Icons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomFoodCategories_Icons_IconId",
                table: "CustomFoodCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_GlobalFoodCategories_Icons_IconId",
                table: "GlobalFoodCategories");

            migrationBuilder.DropTable(
                name: "Icons");

            migrationBuilder.DropIndex(
                name: "IX_GlobalFoodCategories_IconId",
                table: "GlobalFoodCategories");

            migrationBuilder.DropIndex(
                name: "IX_CustomFoodCategories_IconId",
                table: "CustomFoodCategories");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "GlobalFoodCategories");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "CustomFoodCategories");

            migrationBuilder.AddColumn<string>(
                name: "SvgIcon",
                table: "GlobalFoodCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SvgIcon",
                table: "CustomFoodCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomFoodCategories_GlobalFoodCategories_GlobalCategoryId",
                table: "CustomFoodCategories",
                column: "GlobalCategoryId",
                principalTable: "GlobalFoodCategories",
                principalColumn: "Id");
        }
    }
}
