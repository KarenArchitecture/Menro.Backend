using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Seed.Demo.Seeders;

public class DemoRatingSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;

    private readonly Random _rand = new(42);

    public DemoRatingSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        if (await _db.RestaurantRatings.AnyAsync())
            return;

        var users = await _db.Users.ToListAsync();

        var restaurants = await _db.Restaurants.ToListAsync();

        var ratings = new List<RestaurantRating>();

        foreach (var restaurant in restaurants)
        {
            var voters = users
                .Where(x => x.Id != restaurant.OwnerUserId)
                .OrderBy(_ => Guid.NewGuid())
                .Take(_rand.Next(3, 8))
                .ToList();

            foreach (var user in voters)
            {
                ratings.Add(new RestaurantRating
                {
                    RestaurantId = restaurant.Id,
                    UserId = user.Id,
                    Score = _rand.Next(3, 6),
                    CreatedAt =
                        DateTime.UtcNow.AddDays(
                            -_rand.Next(0, 60))
                });
            }
        }

        await _db.RestaurantRatings.AddRangeAsync(ratings);

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Restaurant ratings seeded.");
    }
}