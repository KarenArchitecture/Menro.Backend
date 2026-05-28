using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Menro.Infrastructure.Data.Seed.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class GlobalFoodCategorySeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    public GlobalFoodCategorySeeder(MenroDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (await _db.GlobalFoodCategories.AnyAsync())
        {
            Console.WriteLine(
                "[Seed] Global food categories already seeded.");

            return;
        }

        await _db.GlobalFoodCategories.AddRangeAsync(
            GlobalFoodCategorySeedData.Data);

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[Seed] {GlobalFoodCategorySeedData.Data.Count} global food categories seeded.");
    }
}