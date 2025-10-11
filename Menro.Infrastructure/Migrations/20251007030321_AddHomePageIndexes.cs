using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsActive",
                table: "Restaurants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsActive_IsApproved",
                table: "Restaurants",
                columns: new[] { "IsActive", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsApproved",
                table: "Restaurants",
                column: "IsApproved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsActive",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsActive_IsApproved",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsApproved",
                table: "Restaurants");
        }
    }
}
