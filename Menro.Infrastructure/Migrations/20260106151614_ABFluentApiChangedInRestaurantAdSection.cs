using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ABFluentApiChangedInRestaurantAdSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantAds_PlacementType_Status_StartDate_EndDate",
                table: "RestaurantAds");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantAds_PlacementType_Status_CreatedAt",
                table: "RestaurantAds",
                columns: new[] { "PlacementType", "Status", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "StartDate", "EndDate", "BillingType", "ConsumedUnits", "PurchasedUnits", "RestaurantId" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantAds_PlacementType_Status_Id",
                table: "RestaurantAds",
                columns: new[] { "PlacementType", "Status", "Id" })
                .Annotation("SqlServer:Include", new[] { "StartDate", "EndDate", "BillingType", "ConsumedUnits", "PurchasedUnits", "RestaurantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantAds_PlacementType_Status_CreatedAt",
                table: "RestaurantAds");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantAds_PlacementType_Status_Id",
                table: "RestaurantAds");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantAds_PlacementType_Status_StartDate_EndDate",
                table: "RestaurantAds",
                columns: new[] { "PlacementType", "Status", "StartDate", "EndDate" });
        }
    }
}
