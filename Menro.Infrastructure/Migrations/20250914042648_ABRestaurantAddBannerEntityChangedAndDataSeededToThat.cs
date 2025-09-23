using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABRestaurantAddBannerEntityChangedAndDataSeededToThat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommercialText",
                table: "RestaurantAdBanners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsumedViews",
                table: "RestaurantAdBanners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "RestaurantAdBanners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PurchasedViews",
                table: "RestaurantAdBanners",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommercialText",
                table: "RestaurantAdBanners");

            migrationBuilder.DropColumn(
                name: "ConsumedViews",
                table: "RestaurantAdBanners");

            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "RestaurantAdBanners");

            migrationBuilder.DropColumn(
                name: "PurchasedViews",
                table: "RestaurantAdBanners");
        }
    }
}
