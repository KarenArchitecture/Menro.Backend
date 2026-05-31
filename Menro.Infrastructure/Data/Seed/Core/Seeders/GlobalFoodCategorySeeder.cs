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
    public int Order => SeedOrder.GlobalFoodCategory;
    public async Task SeedAsync()
    {
        var existing = await _db.GlobalFoodCategories
            .ToListAsync();

        foreach (var seed in GlobalFoodCategorySeedData.Data)
        {
            var item = existing.FirstOrDefault(x => x.Name == seed.Name);

            if (item == null)
            {
                await _db.GlobalFoodCategories.AddAsync(seed);
            }
            else
            {
                // sync fields (if needed)
                item.IconId = seed.IconId;
                item.DisplayOrder = seed.DisplayOrder;
                item.IsActive = seed.IsActive;
            }
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] GlobalFoodCategories synced.");
    }
}