using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Seed.Demo.Seeders;

public class DemoOrderSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly UserManager<User> _userManager;

    private readonly Random _rand = new(42);

    public DemoOrderSeeder(
        MenroDbContext db,
        UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        if (await _db.Orders.AnyAsync())
            return;

        const string phone = "09121112233";

        var customer = await _db.Users
            .FirstOrDefaultAsync(x =>
                x.PhoneNumber == phone);

        if (customer == null)
        {
            customer = new User
            {
                UserName = phone,
                PhoneNumber = phone,
                FullName = "مشتری نمونه"
            };

            await _userManager.CreateAsync(
                customer,
                "Customer123!");

            await _userManager.AddToRoleAsync(
                customer,
                SD.Role_Customer);
        }

        var restaurants = await _db.Restaurants
            .Take(6)
            .ToListAsync();

        foreach (var restaurant in restaurants)
        {
            var foods = await _db.Foods
                .Where(x => x.RestaurantId == restaurant.Id)
                .Take(3)
                .ToListAsync();

            if (!foods.Any())
                continue;

            var order = new Order
            {
                UserId = customer.Id,
                RestaurantId = restaurant.Id,

                RestaurantOrderNumber =
                    _rand.Next(1, 500),

                Status = OrderStatus.Completed,

                CreatedAt =
                    DateTime.UtcNow.AddDays(
                        -_rand.Next(1, 20)),

                TotalPrice = foods.Sum(x => x.Price),

                OrderItems = foods.Select(x =>
                    new OrderItem
                    {
                        FoodId = x.Id,
                        Quantity = 1,
                        UnitPrice = x.Price,
                        TitleSnapshot = x.Name
                    }).ToList()
            };

            _db.Orders.Add(order);
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Demo orders seeded.");
    }
}