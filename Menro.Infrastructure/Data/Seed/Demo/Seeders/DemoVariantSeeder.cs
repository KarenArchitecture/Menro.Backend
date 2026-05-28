using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoVariantSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    private readonly Random _rand = new(42);

    public DemoVariantSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (await _db.FoodVariants.AnyAsync())
        {
            Console.WriteLine("[Seed] Variants already seeded.");
            return;
        }

        var foods = await _db.Foods.ToListAsync();

        var variants = new List<FoodVariant>();
        var addons = new List<FoodAddon>();

        foreach (var food in foods)
        {
            var normal = new FoodVariant
            {
                FoodId = food.Id,
                Name = "معمولی",
                Price = food.Price,
                IsDefault = true
            };

            variants.Add(normal);

            var special = new FoodVariant
            {
                FoodId = food.Id,
                Name = "ویژه",
                Price = food.Price + 50000
            };

            variants.Add(special);
        }

        await _db.FoodVariants.AddRangeAsync(variants);

        await _db.SaveChangesAsync();

        var dbVariants = await _db.FoodVariants.ToListAsync();

        foreach (var variant in dbVariants)
        {
            addons.Add(new FoodAddon
            {
                FoodVariantId = variant.Id,
                Name = "پنیر اضافه",
                ExtraPrice = 15000
            });

            addons.Add(new FoodAddon
            {
                FoodVariantId = variant.Id,
                Name = "سس مخصوص",
                ExtraPrice = 10000
            });
        }

        await _db.FoodAddons.AddRangeAsync(addons);

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Demo variants seeded.");
    }
}