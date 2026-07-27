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

    private static readonly string[] DemoCustomerPhones =
    {
        "09121112233",
        "09121112234",
        "09121112235",
        "09121112236",
        "09121112237",
    };

    // Weighted toward Completed so order-history testing has enough entries,
    // but still covers every stage of the admin order-status flow.
    private static readonly OrderStatus[] StatusPool =
    {
        OrderStatus.Pending,
        OrderStatus.Confirmed,
        OrderStatus.Delivered,
        OrderStatus.Paid,
        OrderStatus.Completed,
        OrderStatus.Completed,
        OrderStatus.Cancelled,
    };

    public DemoOrderSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public int Order => SeedOrder.Order;

    public async Task SeedAsync()
    {
        var demoCustomers = await _db.Users
            .Where(u => DemoCustomerPhones.Contains(u.PhoneNumber))
            .ToListAsync();

        if (demoCustomers.Count == 0)
        {
            Console.WriteLine("[Seed] No demo customers found. Skip orders seeding.");
            return;
        }

        var demoCustomerIds = demoCustomers.Select(c => c.Id).ToList();

        if (await _db.Orders.AnyAsync(o => o.UserId != null && demoCustomerIds.Contains(o.UserId)))
        {
            Console.WriteLine("[Seed] Demo orders already seeded.");
            return;
        }

        var allVariants = await _db.FoodVariants
            .Include(v => v.Addons)
            .ToListAsync();

        var restaurantInfos = await _db.Restaurants
            .Where(x => x.IsActive && !x.IsDeleted)
            .Select(x => new { x.Id, x.TableCount })
            .ToListAsync();

        if (restaurantInfos.Count == 0)
        {
            Console.WriteLine("[Seed] No restaurants found. Skip orders seeding.");
            return;
        }

        int dayOffset = 0;

        foreach (var customer in demoCustomers)
        {
            var orderCount = _rand.Next(3, 7);

            for (int n = 0; n < orderCount; n++)
            {
                var info = restaurantInfos[_rand.Next(restaurantInfos.Count)];

                var foods = await _db.Foods
                    .Where(f => f.RestaurantId == info.Id && f.IsAvailable && !f.IsDeleted)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(_rand.Next(2, 5))
                    .ToListAsync();

                if (!foods.Any())
                    continue;

                int totalAmount = 0;
                var orderItems = new List<OrderItem>();

                int? tableNumber = (_rand.NextDouble() < 0.30 || info.TableCount <= 0)
                    ? null
                    : _rand.Next(1, info.TableCount + 1);

                foreach (var food in foods)
                {
                    int quantity = _rand.Next(1, 3);
                    var variantsForFood = allVariants.Where(v => v.FoodId == food.Id).ToList();

                    if (variantsForFood.Count == 0)
                    {
                        // Defensive fallback — shouldn't happen once
                        // FoodDefaultVariantSeeder has run, but kept safe.
                        int unitPrice = food.Price;
                        totalAmount += unitPrice * quantity;

                        orderItems.Add(new OrderItem
                        {
                            FoodId = food.Id,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TitleSnapshot = food.Name,
                            ImageUrlSnapshot = food.ImageUrl,
                        });

                        continue;
                    }

                    var chosenVariant = variantsForFood.FirstOrDefault(v => v.IsDefault == true)
                        ?? variantsForFood.OrderBy(_ => Guid.NewGuid()).First();

                    var selectedAddons = chosenVariant.Addons?
                        .Where(_ => _rand.NextDouble() < 0.45)
                        .ToList() ?? new List<FoodAddon>();

                    int addonsSum = selectedAddons.Sum(a => a.ExtraPrice);
                    int finalUnitPrice = chosenVariant.Price + addonsSum;
                    totalAmount += finalUnitPrice * quantity;

                    orderItems.Add(new OrderItem
                    {
                        FoodId = food.Id,
                        FoodVariantId = chosenVariant.Id,
                        Quantity = quantity,
                        UnitPrice = finalUnitPrice,
                        TitleSnapshot = $"{food.Name} - {chosenVariant.Name}",
                        VariantTitleSnapshot = chosenVariant.Name,
                        ImageUrlSnapshot = food.ImageUrl,
                        Extras = selectedAddons.Select(a => new OrderItemExtra
                        {
                            FoodAddonId = a.Id,
                            AddonTitleSnapshot = a.Name,
                            ExtraPrice = a.ExtraPrice,
                            Quantity = 1,
                        }).ToList()
                    });
                }

                var lastNumber = await _db.Orders
                    .Where(o => o.RestaurantId == info.Id)
                    .Select(o => (int?)o.RestaurantOrderNumber)
                    .MaxAsync() ?? 0;

                var order = new Order
                {
                    UserId = customer.Id,
                    RestaurantId = info.Id,
                    RestaurantOrderNumber = lastNumber + 1,
                    TableNumber = tableNumber,
                    Status = StatusPool[_rand.Next(StatusPool.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-dayOffset++).AddHours(-_rand.Next(0, 20)),
                    TotalPrice = totalAmount,
                    OrderItems = orderItems
                };

                _db.Orders.Add(order);

                // Save per-order so RestaurantOrderNumber (unique per
                // restaurant) is computed correctly against already-saved
                // rows for the next iteration.
                await _db.SaveChangesAsync();
            }
        }

        Console.WriteLine("[Seed] Demo orders seeded for all demo customers.");
    }
}