using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoFavoriteFoodSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly Random _rand = new(42);

    public DemoFavoriteFoodSeeder(MenroDbContext db)
    {
        _db = db;
    }

    public int Order => SeedOrder.FavoriteFood;

    public async Task SeedAsync()
    {
        var demoPhone = ToE164("09121112233");

        var customer = await _db.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == demoPhone);

        if (customer == null)
        {
            Console.WriteLine("[Seed] Demo customer not found. Skip favorite foods.");
            return;
        }

        if (await _db.FavoriteFoods.AnyAsync(x => x.UserId == customer.Id))
        {
            Console.WriteLine("[Seed] Favorite foods already seeded.");
            return;
        }

        var restaurants = await _db.Restaurants
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(_ => Guid.NewGuid())
            .ToListAsync();

        var favorites = new List<FavoriteFood>();

        foreach (var restaurant in restaurants)
        {
            var food = await _db.Foods
                .Where(x =>
                    x.RestaurantId == restaurant.Id &&
                    x.IsAvailable &&
                    !x.IsDeleted)
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (food == null)
                continue;

            favorites.Add(new FavoriteFood
            {
                UserId = customer.Id,
                FoodId = food.Id
            });

            if (favorites.Count >= 12)
                break;
        }

        if (favorites.Count == 0)
        {
            Console.WriteLine("[Seed] No foods found for favorites.");
            return;
        }

        await _db.FavoriteFoods.AddRangeAsync(favorites);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[Seed] {favorites.Count} favorite foods seeded.");
    }

    // 🔧 Local, self-contained conversion — deliberately NOT the shared
    // Utilities class. The demo customer is stored with a "+98..."
    // PhoneNumber (to match what AuthController looks up at login time),
    // so this lookup must use the same form.
    private static string ToE164(string rawPhone) => "+98" + rawPhone[1..];
}