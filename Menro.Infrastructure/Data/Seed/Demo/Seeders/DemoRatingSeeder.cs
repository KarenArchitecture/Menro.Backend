using Menro.Application.Common.Helpers;
using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoRatingSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly Random _rand = new(42);

    private static readonly string[] DemoCustomerPhonesRaw =
    {
        "09121112233",
        "09121112234",
        "09121112235",
        "09121112236",
        "09121112237",
    };

    public DemoRatingSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public int Order => SeedOrder.Rating;

    public async Task SeedAsync()
    {
        var demoCustomerPhones = DemoCustomerPhonesRaw
            .Select(PhoneNumberHelper.ToStorageFormat)
            .ToList();

        var allUsers = await _db.Users.ToListAsync();

        // Excludes BOTH demo customer accounts (0912111xxxx) AND every
        // seeded restaurant owner account (0912100000x / legacy 09120000001)
        // from the random voter pool. Anyone who might realistically log
        // in to manually test the rating flow must start unrated.
        var restaurants = await _db.Restaurants.ToListAsync();
        var ownerUserIds = restaurants
            .Where(r => !string.IsNullOrWhiteSpace(r.OwnerUserId))
            .Select(r => r.OwnerUserId!)
            .ToHashSet();

        var voterPool = allUsers
            .Where(u => !ownerUserIds.Contains(u.Id))
            .Where(u => u.PhoneNumber == null || !demoCustomerPhones.Contains(u.PhoneNumber))
            .ToList();

        var foods = await _db.Foods.ToListAsync();

        /* =========================
           Restaurant Ratings
        ========================= */
        foreach (var restaurant in restaurants)
        {
            if (await _db.RestaurantRatings.AnyAsync(x => x.RestaurantId == restaurant.Id))
                continue;

            int howMany = _rand.Next(3, 8);
            var voters = voterPool
                .Where(u => u.Id != restaurant.OwnerUserId)
                .OrderBy(_ => Guid.NewGuid())
                .Take(howMany)
                .ToList();

            foreach (var user in voters)
            {
                _db.RestaurantRatings.Add(new RestaurantRating
                {
                    RestaurantId = restaurant.Id,
                    UserId = user.Id,
                    Score = _rand.Next(3, 6),
                    CreatedAt = DateTime.UtcNow.AddDays(-_rand.Next(0, 60))
                });
            }
        }

        /* =========================
           Food Ratings
        ========================= */
        foreach (var food in foods)
        {
            if (await _db.FoodRatings.AnyAsync(fr => fr.FoodId == food.Id))
                continue;

            int howMany = _rand.Next(2, 7);
            var voters = voterPool
                .OrderBy(_ => Guid.NewGuid())
                .Take(howMany)
                .ToList();

            foreach (var user in voters)
            {
                _db.FoodRatings.Add(new FoodRating
                {
                    FoodId = food.Id,
                    UserId = user.Id,
                    Score = _rand.Next(3, 6),
                    CreatedAt = DateTime.UtcNow.AddDays(-_rand.Next(0, 45))
                });
            }
        }

        await _db.SaveChangesAsync();
        Console.WriteLine("[Seed] Demo ratings seeded (excluding all demo owner/customer test accounts as voters).");
    }
}