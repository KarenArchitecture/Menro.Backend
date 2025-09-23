using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // instead of migrationBuilder.CreateIndex(...):
            migrationBuilder.Sql(@"
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_FoodRatings_FoodId'
                  AND object_id = OBJECT_ID(N'[dbo].[FoodRatings]')
            )
            BEGIN
                CREATE INDEX [IX_FoodRatings_FoodId] ON [dbo].[FoodRatings]([FoodId]);
            END
            ");

            // keep your other new indexes here, and apply the same pattern
            // for any index that might already exist.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_FoodRatings_FoodId'
                  AND object_id = OBJECT_ID(N'[dbo].[FoodRatings]')
            )
            BEGIN
                DROP INDEX [IX_FoodRatings_FoodId] ON [dbo].[FoodRatings];
            END
            ");
        }

    }
}
