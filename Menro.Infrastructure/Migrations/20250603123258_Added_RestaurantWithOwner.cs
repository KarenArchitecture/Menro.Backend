using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_RestaurantWithOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Restaurants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Restaurants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_OwnerUserId",
                table: "Restaurants",
                column: "OwnerUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AspNetUsers_OwnerUserId",
                table: "Restaurants",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AspNetUsers_OwnerUserId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_OwnerUserId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Restaurants");

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "Address", "BankAccountNumber", "BannerImageUrl", "Name", "NationalCode", "OwnerFullName", "PhoneNumber", "RestaurantCategoryId", "ShebaNumber" },
                values: new object[] { 1, "خیابان ولیعصر، پلاک ۱۲۳", "1234567890", null, "کافه منرو", "0012345678", "امین منرو", "09123456789", 5, "IR820540102680020817909002" });
        }
    }
}
