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
    public int Order => SeedOrder.Variant;
    public async Task SeedAsync()
    {
        var foods = await _db.Foods
            .Include(f => f.Variants)
            .ThenInclude(v => v.Addons)
            .ToListAsync();

        foreach (var food in foods)
        {
            if (food.Variants != null && food.Variants.Any())
                continue;

            double r = _rand.NextDouble();

            int variantCount =
                (r < 0.30) ? 0 :
                (r < 0.50) ? 1 :
                (r < 0.80) ? 2 : 3;

            if (variantCount == 0)
                continue;

            var basePrice = Math.Max(5000, food.Price);

            var variants = new List<FoodVariant>();

            if (variantCount >= 1)
            {
                variants.Add(new FoodVariant
                {
                    Name = "معمولی",
                    Price = basePrice,
                    FoodId = food.Id
                });
            }

            if (variantCount >= 2)
            {
                variants.Add(new FoodVariant
                {
                    Name = "ویژه",
                    Price = basePrice + (int)(basePrice * 0.15),
                    FoodId = food.Id
                });
            }

            if (variantCount == 3)
            {
                variants.Add(new FoodVariant
                {
                    Name = "خانواده",
                    Price = basePrice + (int)(basePrice * 0.30),
                    FoodId = food.Id
                });
            }

            var defaultVariant =
                variants.FirstOrDefault(v => v.Name == "ویژه")
                ?? variants.OrderByDescending(v => v.Price).First();

            defaultVariant.IsDefault = true;

            _db.FoodVariants.AddRange(variants);
            await _db.SaveChangesAsync();

            foreach (var v in variants)
            {
                double addonRand = _rand.NextDouble();

                int addonsToCreate =
                    (addonRand < 0.40) ? 0 :
                    (addonRand < 0.70) ? 1 :
                    (addonRand < 0.90) ? 2 : 3;

                if (addonsToCreate == 0)
                    continue;

                for (int i = 0; i < addonsToCreate; i++)
                {
                    var addon = new FoodAddon
                    {
                        FoodVariantId = v.Id,
                        Name = i switch
                        {
                            0 => "پنیر اضافه",
                            1 => "سس مخصوص",
                            2 => "سیب‌زمینی کوچک",
                            _ => "تاپینگ ویژه"
                        },
                        ExtraPrice = 8000 + _rand.Next(0, 7000)
                    };

                    _db.FoodAddons.Add(addon);
                }
            }
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Demo variants seeded.");
    }
}