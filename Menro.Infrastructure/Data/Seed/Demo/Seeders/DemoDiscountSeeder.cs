using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Seed.Demo.Seeders;

public class DemoDiscountSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    private readonly Random _rand = new(42);

    public DemoDiscountSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (await _db.Discounts.AnyAsync())
        {
            Console.WriteLine(
                "[Seed] Demo discounts already seeded.");

            return;
        }

        var percentPool = new[]
        {
            10m,
            15m,
            20m,
            25m,
            30m
        };

        var restaurants = await _db.Restaurants
            .Include(x => x.Foods)
            .ToListAsync();

        foreach (var restaurant in restaurants)
        {
            if (!restaurant.Foods.Any())
                continue;

            bool hasDiscount =
                _rand.NextDouble() < 0.35;

            if (!hasDiscount)
                continue;

            decimal? maxDiscount = null;

            var discountedFoods = restaurant.Foods
                .OrderBy(_ => Guid.NewGuid())
                .Take(
                    _rand.Next(
                        1,
                        Math.Min(4, restaurant.Foods.Count)))
                .ToList();

            foreach (var food in discountedFoods)
            {
                if (_rand.NextDouble() < 0.5)
                {
                    var percent =
                        percentPool[
                            _rand.Next(percentPool.Length)];

                    var discount = new Discount
                    {
                        Scope = DiscountScope.Food,

                        RestaurantId = restaurant.Id,

                        FoodId = food.Id,

                        ValueType =
                            DiscountValueType.Percent,

                        Value = percent,

                        StartDate =
                            DateTime.UtcNow.AddDays(
                                -_rand.Next(0, 3)),

                        EndDate =
                            DateTime.UtcNow.AddDays(
                                _rand.Next(5, 15)),

                        IsActive = true,
                        IsDeleted = false,

                        CreatedAt = DateTime.UtcNow
                    };

                    _db.Discounts.Add(discount);

                    if (!maxDiscount.HasValue ||
                        percent > maxDiscount.Value)
                    {
                        maxDiscount = percent;
                    }
                }
            }

            if (maxDiscount.HasValue &&
                maxDiscount.Value >= 10)
            {
                restaurant.Description +=
                    $" 🔥 تا {maxDiscount.Value}% تخفیف";
            }
        }

        await _db.SaveChangesAsync();

        Console.WriteLine(
            "[Seed] Demo discounts seeded.");
    }
}
```
