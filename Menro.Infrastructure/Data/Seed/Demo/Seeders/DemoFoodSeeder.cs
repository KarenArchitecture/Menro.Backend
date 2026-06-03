//using Menro.Domain.Entities;
//using Menro.Infrastructure.Data.Seed.Contracts;
//using Microsoft.EntityFrameworkCore;

//namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

//public class DemoFoodSeeder : IDataSeeder
//{
//    private readonly MenroDbContext _db;
//    private readonly Random _rand = new(42);

//    public DemoFoodSeeder(MenroDbContext db)
//    {
//        _db = db;
//    }

//    public int Order => SeedOrder.Food;

//    private const string FoodImage = "/img/drink.png";

//    private static readonly Dictionary<string, string[]> FoodNamesByGlobal = new()
//    {
//        ["پیتزا"] =
//        [
//            "پیتزا ناپلی",
//            "پیتزا پپرونی",
//            "پیتزا مارگاریتا",
//            "پیتزا چهار فصل",
//            "پیتزا باربیکیو",
//            "پیتزا قارچ و مرغ"
//        ],

//        ["برگر"] =
//        [
//            "برگر کلاسیک",
//            "چیزبرگر دوبل",
//            "برگر ویژه",
//            "چیکن برگر",
//            "اسموکی برگر"
//        ],

//        ["نوشیدنی گرم"] =
//        [
//            "اسپرسو",
//            "کاپوچینو",
//            "لاته",
//            "موکا",
//            "هات چاکلت"
//        ],

//        ["نوشیدنی سرد"] =
//        [
//            "موهیتو",
//            "لیموناد",
//            "شیک شکلات",
//            "آیس لاته",
//            "موکتل بری"
//        ],

//        ["سالاد"] =
//        [
//            "سالاد سزار",
//            "سالاد یونانی",
//            "سالاد فصل",
//            "سالاد مرغ"
//        ],

//        ["دسر"] =
//        [
//            "چیزکیک",
//            "براونی",
//            "پاناکوتا",
//            "تیرامیسو"
//        ]
//    };

//    private static (int min, int max) PriceRangeFor(string categoryName)
//    {
//        return categoryName switch
//        {
//            "پیتزا" => (280_000, 620_000),
//            "برگر" => (220_000, 480_000),
//            "نوشیدنی گرم" => (80_000, 180_000),
//            "نوشیدنی سرد" => (90_000, 220_000),
//            "سالاد" => (160_000, 340_000),
//            "دسر" => (120_000, 260_000),

//            _ => (100_000, 300_000)
//        };
//    }

//    public async Task SeedAsync()
//    {
//        var restaurants = await _db.Restaurants
//            .Include(r => r.FoodCategories)
//            .ToListAsync();

//        if (!restaurants.Any())
//        {
//            Console.WriteLine("[Seed] No restaurants found.");
//            return;
//        }

//        var globalCategories = await _db.GlobalFoodCategories
//            .Where(x => x.IsActive)
//            .ToListAsync();

//        foreach (var restaurant in restaurants)
//        {
//            bool alreadySeeded = await _db.Foods
//                .AnyAsync(x => x.RestaurantId == restaurant.Id);

//            if (alreadySeeded)
//                continue;

//            var selectedGlobals = globalCategories
//                .OrderBy(_ => Guid.NewGuid())
//                .Take(_rand.Next(4, 7))
//                .ToList();

//            var categories = new List<CustomFoodCategory>();

//            foreach (var globalCat in selectedGlobals)
//            {
//                var customCat = new CustomFoodCategory
//                {
//                    Name = globalCat.Name,
//                    IconId = globalCat.IconId,
//                    RestaurantId = restaurant.Id,
//                    GlobalCategoryId = globalCat.Id,
//                    IsAvailable = true,
//                    IsDeleted = false
//                };

//                categories.Add(customCat);
//            }

//            await _db.CustomFoodCategories.AddRangeAsync(categories);
//            await _db.SaveChangesAsync();

//            var foods = new List<Food>();

//            foreach (var category in categories)
//            {
//                int foodCount = _rand.Next(6, 10);

//                var foodPool =
//                    FoodNamesByGlobal.TryGetValue(category.Name, out var pool)
//                    ? pool
//                    : new[]
//                    {
//                        "آیتم ویژه",
//                        "غذای سرآشپز",
//                        "پیشنهاد ویژه",
//                        "غذای محبوب"
//                    };

//                var priceRange = PriceRangeFor(category.Name);

//                for (int i = 0; i < foodCount; i++)
//                {
//                    foods.Add(new Food
//                    {
//                        Name = foodPool[i % foodPool.Length],

//                        Ingredients =
//                            "مواد اولیه تازه و با کیفیت",

//                        Price = _rand.Next(
//                            priceRange.min,
//                            priceRange.max),

//                        RestaurantId = restaurant.Id,

//                        CustomFoodCategoryId = category.Id,

//                        GlobalFoodCategoryId =
//                            category.GlobalCategoryId,

//                        ImageUrl = FoodImage,

//                        IsAvailable = true,
//                        IsDeleted = false,

//                        CreatedAt =
//                            DateTime.UtcNow.AddDays(
//                                -_rand.Next(0, 30))
//                    });
//                }
//            }

//            await _db.Foods.AddRangeAsync(foods);
//            await _db.SaveChangesAsync();
//        }

//        Console.WriteLine("[Seed] Demo foods seeded.");
//    }
//}