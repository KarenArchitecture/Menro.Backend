// Infrastructure/Data/Seed/Core/Seeders/FoodDefaultVariantSeeder.cs
using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders
{
    // Enforces the invariant that every Food must have at least one
    // available variant. Runs on every startup (idempotent — it only
    // touches foods currently missing one), so it heals gaps left by
    // seeders, drop-database resets, or manual DB edits, not just a
    // one-off fix.
    public class FoodDefaultVariantSeeder : IDataSeeder
    {
        private readonly MenroDbContext _db;
        public FoodDefaultVariantSeeder(MenroDbContext db) => _db = db;

        // Must run after DemoFoodSeeder/DemoVariantSeeder. Pick a number
        // higher than any existing SeedOrder value used in this project.
        public int Order => 999;

        public async Task SeedAsync()
        {
            var foodsMissingVariant = await _db.Foods
                .Where(f => !f.IsDeleted && f.IsAvailable)
                .Where(f => !f.Variants.Any(v => !v.IsDeleted && v.IsAvailable))
                .ToListAsync();

            if (foodsMissingVariant.Count == 0)
            {
                Console.WriteLine("[Seed] All foods already have an available variant.");
                return;
            }

            foreach (var food in foodsMissingVariant)
            {
                _db.FoodVariants.Add(new FoodVariant
                {
                    FoodId = food.Id,
                    Name = "معمولی",
                    Price = food.Price,
                    IsAvailable = true,
                    IsDeleted = false,
                    IsDefault = true,
                });
            }

            await _db.SaveChangesAsync();
            Console.WriteLine($"[Seed] Backfilled default variants for {foodsMissingVariant.Count} food(s).");
        }
    }
}