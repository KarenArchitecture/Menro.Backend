using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoOrderSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    private readonly Random _rand = new(42);

    public DemoOrderSeeder(MenroDbContext db)
    {
        _db = db;
    }
    public int Order => SeedOrder.Order;
    public async Task SeedAsync()
    {
        var demoPhone = "09121112233";

        var demoCustomer = await _db.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == demoPhone);

        if (demoCustomer == null)
        {
            Console.WriteLine("[Seed] Demo customer not found. Skip orders seeding.");
            return;
        }

        if (await _db.Orders.AnyAsync(o => o.UserId == demoCustomer.Id))
        {
            Console.WriteLine("[Seed] Demo orders already seeded.");
            return;
        }

        var allVariants = await _db.FoodVariants
            .Include(v => v.Addons)
            .ToListAsync();

        var restaurantInfos = await _db.Restaurants
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(_ => Guid.NewGuid())
            .Select(x => new { x.Id, x.TableCount })
            .Take(8)
            .ToListAsync();

        int dayOffset = 0;

        foreach (var info in restaurantInfos)
        {
            var foods = await _db.Foods
                .Where(f =>
                    f.RestaurantId == info.Id &&
                    f.IsAvailable &&
                    !f.IsDeleted)
                .OrderBy(_ => Guid.NewGuid())
                .Take(_rand.Next(2, 5))
                .ToListAsync();

            if (!foods.Any())
                continue;

            decimal totalAmount = 0m;

            var orderItems = new List<OrderItem>();

            int? tableNumber;

            if (_rand.NextDouble() < 0.30 || info.TableCount <= 0)
            {
                tableNumber = null;
            }
            else
            {
                tableNumber = _rand.Next(1, info.TableCount + 1);
            }

            foreach (var food in foods)
            {
                int quantity = _rand.Next(1, 3);

                var variantsForFood = allVariants
                    .Where(v => v.FoodId == food.Id)
                    .ToList();

                if (variantsForFood.Count == 0)
                {
                    decimal unitPrice = food.Price;

                    totalAmount += unitPrice * quantity;

                    orderItems.Add(new OrderItem
                    {
                        FoodId = food.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TitleSnapshot = food.Name
                    });

                    continue;
                }

                var chosenVariant =
                    variantsForFood.FirstOrDefault(v => v.IsDefault == true)
                    ?? variantsForFood.OrderBy(_ => Guid.NewGuid()).First();

                var selectedAddons = chosenVariant.Addons?
                    .Where(_ => _rand.NextDouble() < 0.45)
                    .ToList()
                    ?? new List<FoodAddon>();

                int addonsSum = selectedAddons.Sum(a => a.ExtraPrice);

                decimal finalUnitPrice = chosenVariant.Price + addonsSum;

                totalAmount += finalUnitPrice * quantity;

                orderItems.Add(new OrderItem
                {
                    FoodId = food.Id,
                    FoodVariantId = chosenVariant.Id,
                    Quantity = quantity,
                    UnitPrice = finalUnitPrice,
                    TitleSnapshot = $"{food.Name} - {chosenVariant.Name}",
                    VariantTitleSnapshot = chosenVariant.Name,
                    Extras = selectedAddons.Select(a => new OrderItemExtra
                    {
                        FoodAddonId = a.Id,
                        AddonTitleSnapshot = a.Name,
                        ExtraPrice = a.ExtraPrice
                    }).ToList()
                });
            }

            var lastNumber = await _db.Orders
                .Where(o => o.RestaurantId == info.Id)
                .Select(o => (int?)o.RestaurantOrderNumber)
                .MaxAsync() ?? 0;

            var order = new Order
            {
                UserId = demoCustomer.Id,
                RestaurantId = info.Id,
                RestaurantOrderNumber = lastNumber + 1,
                TableNumber = tableNumber,
                Status = OrderStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-dayOffset++),
                TotalPrice = totalAmount,
                OrderItems = orderItems
            };

            _db.Orders.Add(order);
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Demo orders seeded.");
    }
}