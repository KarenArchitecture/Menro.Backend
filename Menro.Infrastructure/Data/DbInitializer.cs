using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Menro.Application.Extensions; // for TransliterateToEnglish()
using Menro.Application.Restaurants.Services.Interfaces; // for IRestaurantService

namespace Menro.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MenroDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IRestaurantService _restaurantService;

        /* ---------- knobs you can tune ---------- */
        private const int RestaurantsToCreate = 12;
        private const int MinCatsPerRestaurant = 4;
        private const int MaxCatsPerRestaurant = 6;
        private const int MinFoodsPerCategory = 6;
        private const int MaxFoodsPerCategory = 9;
        private const int MinRestRatings = 3;
        private const int MaxRestRatings = 7;
        private const int MinFoodRatings = 2;
        private const int MaxFoodRatings = 6;
        private const int TargetAdBanners = 5;

        private static readonly string[] BannerImages = { "/img/top-banner.png", "/img/optcropban.jpg", "/img/res-slider.jpg" };
        private static readonly string[] CarouselImages = { "/img/res-slider.jpg", "/img/optcropban.jpg" };
        private static readonly string[] CardImages = { "/img/res-card-1.png", "/img/res-card-2.png" };
        private static readonly string[] ShopBannerImages = { "/img/ad-banner-1.jpg", "/img/ad-banner-2.png" };
        private static readonly string[] Logos = { "/img/logo-orange.png", "/img/logo-green.png" };
        private static readonly string FoodFallbackImage = "/img/drink.png";

        public DbInitializer(
            MenroDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            IRestaurantService restaurantService)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _restaurantService = restaurantService;
        }

        // Seed Icons
        private async Task SeedIconsAsync()
        {
            if (await _db.Icons.AnyAsync()) return;
            _db.Icons.AddRange(IconSeed.Data);
            await _db.SaveChangesAsync();
        }

        // Seed GlobalFoodCategories
        private async Task SeedGlobalFoodCategoriesAsync()
        {
            if (await _db.GlobalFoodCategories.AnyAsync()) return;
            _db.GlobalFoodCategories.AddRange(GlobalFoodCategorySeed.Data);
            await _db.SaveChangesAsync();
        }

        // Food name pools for variety
        private static readonly Dictionary<string, string[]> FoodNamesByGlobal = new()
        {
            ["پیتزا"] = new[] { "پیتزا ناپلی", "پیتزا پپرونی", "پیتزا مارگاریتا", "پیتزا چهار فصل", "پیتزا قارچ و مرغ", "پیتزا ویژه", "پیتزا باربیکیو" },
            ["برگر"] = new[] { "چیزبرگر دوبل", "برگر کلاسیک", "اسموکی برگر", "برگر قارچ‌سوخاری", "چیکن برگر", "برگر ویژه منرو" },
            ["نوشیدنی گرم"] = new[] { "اسپرسو", "کاپوچینو", "لاته", "موکا", "آمریکانو", "هات چاکلت", "چای ماسالا" },
            ["نوشیدنی سرد"] = new[] { "موکتل بری", "لیموناد نعنایی", "موهیتو", "شیک وانیل", "شیک شکلات", "آیس لاته", "آیس آمریکانو" },
            ["سالاد"] = new[] { "سالاد سزار", "سالاد یونانی", "سالاد فصل", "سالاد کینوا", "سالاد مرغ گریل" },
            ["دسر"] = new[] { "براونی شکلاتی", "چیزکیک نیویورکی", "تیرا میسو", "پاناكوتا", "فرانچ‌توست کاراملی" }
        };

        private static (int min, int max) PriceRangeFor(string globalCat) =>
            globalCat switch
            {
                "پیتزا" => (280_000, 620_000),
                "برگر" => (220_000, 480_000),
                "نوشیدنی گرم" => (80_000, 180_000),
                "نوشیدنی سرد" => (90_000, 220_000),
                "سالاد" => (160_000, 340_000),
                "دسر" => (120_000, 260_000),
                _ => (100_000, 300_000)
            };

        private static int NextPrice(Random rnd, (int min, int max) range) =>
            rnd.Next(range.min, range.max + 1);

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("========== Menro Database Initialization ==========");

                var canConnect = await _db.Database.CanConnectAsync();
                Console.WriteLine($"Database connection: {(canConnect ? "OK" : "FAILED")}");

                var appliedMigrations = (await _db.Database.GetAppliedMigrationsAsync()).ToList();
                var pendingMigrations = (await _db.Database.GetPendingMigrationsAsync()).ToList();

                Console.WriteLine($"Applied migrations count: {appliedMigrations.Count}");
                foreach (var migration in appliedMigrations)
                {
                    Console.WriteLine($"  Applied: {migration}");
                }

                Console.WriteLine($"Pending migrations count: {pendingMigrations.Count}");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"  Pending: {migration}");
                }

                Console.WriteLine("Running EF Core migrations...");
                await _db.Database.MigrateAsync();
                Console.WriteLine("EF Core migrations completed.");

                var tablesCount = await _db.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")
                    .SingleAsync();

                Console.WriteLine($"Database tables count: {tablesCount}");

                var iconsTableExists = await _db.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Icons'")
                    .SingleAsync();

                Console.WriteLine($"Icons table exists: {iconsTableExists > 0}");

                if (tablesCount == 0)
                {
                    throw new InvalidOperationException(
                        "Migration finished but no tables were created. This usually means EF Core migrations are missing or not included in the startup project.");
                }

                if (iconsTableExists == 0)
                {
                    Console.WriteLine("Warning: Icons table was not found. If Icons is part of your current model, create a new migration or check existing migrations.");
                }

                Console.WriteLine("Database initialization completed successfully.");
                Console.WriteLine("===================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database initialization failed.");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
