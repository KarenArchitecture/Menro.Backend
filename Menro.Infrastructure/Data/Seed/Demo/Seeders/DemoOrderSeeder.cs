using Menro.Application.Common.Helpers;
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

    private static string BuildInvoiceNumber(Dictionary<(int RestaurantId, int Y, int M, int D), int> counters, int restaurantId, DateTime createdAtUtc)
    {
        var iranOffset = TimeSpan.FromHours(3.5);
        var localCreated = createdAtUtc + iranOffset;
        var pc = new System.Globalization.PersianCalendar();

        var y = pc.GetYear(localCreated);
        var m = pc.GetMonth(localCreated);
        var d = pc.GetDayOfMonth(localCreated);
        var key = (restaurantId, y, m, d);

        counters.TryGetValue(key, out var seq);
        seq += 1;
        counters[key] = seq;

        return $"{y:D4}{m:D2}{d:D2}{seq}";
    }

    // 🔧 Must match DemoCustomerSeeder.Customers' phone list character-for-
    // character (same local/raw 09... format) — both seeders normalize
    // through the same PhoneNumberHelper.ToStorageFormat, so they need to
    // agree on the raw input to land on the exact same +98 value.
    private static readonly string[] DemoCustomerPhonesRaw =
    {
        "09121112233",
        "09121112234",
        "09121112235",
        "09121112236",
        "09121112237",
    };

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
        var demoCustomerPhonesE164 = DemoCustomerPhonesRaw
            .Select(PhoneNumberHelper.ToStorageFormat)
            .ToList();

        var demoCustomers = await _db.Users
            .Where(u => u.PhoneNumber != null && demoCustomerPhonesE164.Contains(u.PhoneNumber))
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
            .Select(x => new
            {
                x.Id,
                TableLabels = x.Tables.Select(t => t.Label).ToList()
            })
            .ToListAsync();

        if (restaurantInfos.Count == 0)
        {
            Console.WriteLine("[Seed] No restaurants found. Skip orders seeding.");
            return;
        }

        var invoiceCounters = new Dictionary<(int, int, int, int), int>();

        int dayOffset = 0;

        // Iterate by RESTAURANT, not by customer, so every restaurant is
        // guaranteed a handful of orders — lets any of the seeded owners
        // log into the admin panel and always find order data for their
        // own restaurant, regardless of random chance.
        foreach (var info in restaurantInfos)
        {
            var foodsForRestaurant = await _db.Foods
                .Where(f => f.RestaurantId == info.Id && f.IsAvailable && !f.IsDeleted)
                .ToListAsync();

            if (!foodsForRestaurant.Any())
                continue;

            var ordersForThisRestaurant = _rand.Next(4, 9); // 4–8 orders per restaurant

            for (int n = 0; n < ordersForThisRestaurant; n++)
            {
                var customer = demoCustomers[_rand.Next(demoCustomers.Count)];

                var maxPick = Math.Min(5, foodsForRestaurant.Count);
                var foods = foodsForRestaurant
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(_rand.Next(2, maxPick + 1))
                    .ToList();

                if (!foods.Any())
                    continue;

                int totalAmount = 0;
                var orderItems = new List<OrderItem>();

                string? tableLabel = (_rand.NextDouble() < 0.30 || info.TableLabels.Count == 0)
                    ? null
                    : info.TableLabels[_rand.Next(info.TableLabels.Count)];

                foreach (var food in foods)
                {
                    int quantity = _rand.Next(1, 3);
                    var variantsForFood = allVariants.Where(v => v.FoodId == food.Id).ToList();

                    if (variantsForFood.Count == 0)
                    {
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

                var createdAt = DateTime.UtcNow.AddDays(-dayOffset++).AddHours(-_rand.Next(0, 20));
                var invoiceNumber = BuildInvoiceNumber(invoiceCounters, info.Id, createdAt);

                var order = new Order
                {
                    UserId = customer.Id,
                    RestaurantId = info.Id,
                    RestaurantOrderNumber = lastNumber + 1,
                    TableLabel = tableLabel,
                    Status = StatusPool[_rand.Next(StatusPool.Length)],
                    InvoiceNumber = invoiceNumber,
                    CreatedAt = createdAt,
                    TotalPrice = totalAmount,
                    OrderItems = orderItems
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
            }
        }

        Console.WriteLine("[Seed] Demo orders seeded across all restaurants.");
    }
}