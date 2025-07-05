using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABAddingRatingAndDiscoutAndSeedingToIt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantRatings_AspNetUsers_UserId1",
                table: "RestaurantRatings");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantRatings_UserId1",
                table: "RestaurantRatings");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "RestaurantRatings");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Restaurants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Restaurants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RestaurantRatings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantRatings_AspNetUsers_UserId",
                table: "RestaurantRatings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantRatings_AspNetUsers_UserId",
                table: "RestaurantRatings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Restaurants");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RestaurantRatings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "RestaurantRatings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantRatings_UserId1",
                table: "RestaurantRatings",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantRatings_AspNetUsers_UserId1",
                table: "RestaurantRatings",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
