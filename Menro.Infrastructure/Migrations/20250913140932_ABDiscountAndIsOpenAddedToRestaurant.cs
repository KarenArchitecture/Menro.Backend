using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABDiscountAndIsOpenAddedToRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoImageUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoImageUrl",
                table: "Restaurants");
        }
    }
}
