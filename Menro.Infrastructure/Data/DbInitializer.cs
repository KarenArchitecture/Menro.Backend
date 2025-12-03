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
                /* ============================================================
                   Database Migration
                ============================================================ */
                if (_db.Database.GetPendingMigrations().Any())
                    await _db.Database.MigrateAsync();

                /* ============================================================
                   Core Seeds (Icons + Globals + Roles + Admin)
                ============================================================ */
                await SeedIconsAsync();
                await SeedGlobalFoodCategoriesAsync();

                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                if (!await _db.Users.AnyAsync(u => u.Email == "MenroAdmin@gmail.com"))
                {
                    var admin = new User
                    {
                        UserName = "MenroAdmin_1",
                        Email = "MenroAdmin@gmail.com",
                        FullName = "مدیر",
                        PhoneNumber = "+989486813486"
                    };
                    await _userManager.CreateAsync(admin, "@Admin123456");
                    await _userManager.AddToRoleAsync(admin, SD.Role_Admin);
                }

                var rand = new Random();

                var globalCats = await _db.GlobalFoodCategories
                    .Where(gc => gc.IsActive)
                    .OrderBy(gc => gc.DisplayOrder)
                    .ToListAsync();

                /* ============================================================
                   Owners + Restaurants
                ============================================================ */
                var restNames = new[]
                {
                    "پیتزا بامبو","کافه مانا","برگرستان","رستوران نوفل‌لوشاتو",
                    "کافه چرخ","پاستا کونتو","سوشی یو","دلمه خانه",
                    "کباب‌سرای پارس","کترینگ سیب","نان و نمک","شیرینی‌سرای گل"
                };

                for (int i = 1; i <= RestaurantsToCreate; i++)
                {
                    string email = $"owner{i}@menro.com";
                    if (await _db.Users.AnyAsync(u => u.Email == email)) continue;

                    var owner = new User
                    {
                        UserName = $"0912{345678 + i}",
                        Email = email,
                        FullName = $"صاحب رستوران {i}",
                        PhoneNumber = $"0912{345678 + i}"
                    };
                    await _userManager.CreateAsync(owner, "Owner123!");
                    await _userManager.AddToRoleAsync(owner, SD.Role_Owner);

                    var restName = restNames[(i - 1) % restNames.Length];
                    var slug = await _restaurantService.GenerateUniqueSlugAsync(restName.TransliterateToEnglish());

                    var restaurant = new Restaurant
                    {
                        Name = restName,
                        Address = $"تهران، خیابان نمونه {i}",
                        OpenTime = new TimeSpan(8 + (i % 4), 0, 0),
                        CloseTime = new TimeSpan(20 + (i % 3), 30, 0),
                        Description = $"توضیح نمونه برای {restName}؛ غذای باکیفیت و سرویس سریع.",
                        NationalCode = (1000000000 + i).ToString(),
                        BankAccountNumber = (2000000000 + i).ToString(),
                        ShebaNumber = $"IR{3000000000 + i}",
                        OwnerUserId = owner.Id,
                        RestaurantCategoryId = (i % 8) + 1,
                        CarouselImageUrl = CarouselImages[(i - 1) % CarouselImages.Length],
                        BannerImageUrl = CardImages[(i - 1) % CardImages.Length],
                        ShopBannerImageUrl = ShopBannerImages[(i - 1) % CardImages.Length],
                        LogoImageUrl = Logos[(i - 1) % Logos.Length],
                        IsFeatured = (i % 3 == 0),
                        IsActive = true,
                        IsApproved = true,
                        Slug = slug,
                        CreatedAt = DateTime.UtcNow.AddDays(-i)
                    };
                    _db.Restaurants.Add(restaurant);
                    await _db.SaveChangesAsync();

                    /* -------------------------
                       Special Custom Categories
                    ------------------------- */
                    var specialCategoryNames = new[] { "پیشنهاد سرآشپز", "پرفروش‌ترین‌ها", "ویژه امروز" };
                    foreach (var specialName in specialCategoryNames)
                    {
                        var selectedGlobalCat = globalCats[rand.Next(globalCats.Count)];

                        var specialCat = new CustomFoodCategory
                        {
                            Name = specialName,
                            IconId = selectedGlobalCat.IconId,
                            RestaurantId = restaurant.Id
                        };
                        _db.CustomFoodCategories.Add(specialCat);
                        await _db.SaveChangesAsync();

                        var count = rand.Next(2, 4);
                        for (int f = 0; f < count; f++)
                        {
                            _db.Foods.Add(new Food
                            {
                                Name = $"{specialName} {f + 1}",
                                Ingredients = "مواد اولیه تازه و با کیفیت",
                                Price = rand.Next(150_000, 400_000),
                                CustomFoodCategoryId = specialCat.Id,
                                RestaurantId = restaurant.Id,
                                ImageUrl = FoodFallbackImage,
                                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 30)),
                                IsAvailable = true
                            });
                        }
                    }
                    await _db.SaveChangesAsync();

                    /* -------------------------
                       Restaurant Categories (after global linking)
                    ------------------------- */
                    var catCount = rand.Next(MinCatsPerRestaurant, MaxCatsPerRestaurant + 1);
                    for (int c = 0; c < catCount; c++)
                    {
                        bool basedOnGlobal = rand.NextDouble() < 0.6;

                        CustomFoodCategory customCat;
                        if (basedOnGlobal && globalCats.Any())
                        {
                            var globalCat = globalCats[rand.Next(globalCats.Count)];
                            customCat = new CustomFoodCategory
                            {
                                Name = globalCat.Name,
                                IconId = globalCat.IconId,
                                RestaurantId = restaurant.Id,
                                GlobalCategoryId = globalCat.Id
                            };
                        }
                        else
                        {
                            customCat = new CustomFoodCategory
                            {
                                Name = $"دسته ویژه {c + 1}",
                                IconId = globalCats[rand.Next(globalCats.Count)].IconId,
                                RestaurantId = restaurant.Id
                            };
                        }
                        string baseName = customCat.Name;
                        int duplicateCounter = 1;
                        while (await _db.CustomFoodCategories.AnyAsync(c => c.RestaurantId == restaurant.Id && c.Name == customCat.Name))
                        {
                            duplicateCounter++;
                            customCat.Name = $"{baseName} {duplicateCounter}";
                        }
                        _db.CustomFoodCategories.Add(customCat);
                        await _db.SaveChangesAsync();

                        int foodCount = rand.Next(MinFoodsPerCategory, MaxFoodsPerCategory + 1);
                        var pool = (customCat.GlobalCategoryId.HasValue &&
                                    FoodNamesByGlobal.TryGetValue(customCat.Name, out var arr))
                                   ? arr
                                   : new[] { "آیتم ویژه", "آیتم محبوب", "غذای سرآشپز" };

                        for (int k = 0; k < foodCount; k++)
                        {
                            _db.Foods.Add(new Food
                            {
                                Name = pool[k % pool.Length],
                                Ingredients = "مواد اولیه تازه و با کیفیت",
                                Price = NextPrice(rand, PriceRangeFor(customCat.Name)),
                                RestaurantId = restaurant.Id,
                                CustomFoodCategoryId = customCat.Id,
                                ImageUrl = FoodFallbackImage,
                                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 45)),
                                IsAvailable = true
                            });
                        }
                        await _db.SaveChangesAsync();
                    }
                } // end restaurant loop

                /* ============================================================
                   Variants + Addons (REALISTIC DISTRIBUTION, NO DUPLICATES)
                ============================================================ */
                var seededFoods = await _db.Foods
                    .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                    .ToListAsync();

                foreach (var food in seededFoods)
                {
                    // --- Skip foods that already have variants (avoid duplication)
                    if (food.Variants != null && food.Variants.Any())
                        continue;

                    // --- Decide variant count realistically:
                    // 30% → no variants
                    // 20% → 1 variant
                    // 30% → 2 variants
                    // 20% → 3 variants
                    double r = rand.NextDouble();
                    int variantCount =
                        (r < 0.30) ? 0 :
                        (r < 0.50) ? 1 :
                        (r < 0.80) ? 2 :
                                      3;

                    if (variantCount == 0)
                        continue;  // this food has no variants at all

                    var basePrice = Math.Max(5000, food.Price);

                    // Build variant list
                    var variants = new List<FoodVariant>();

                    if (variantCount >= 1)
                    {
                        variants.Add(new FoodVariant
                        {
                            Name = "معمولی",
                            Price = basePrice,
                            FoodId = food.Id
                        });
                    }
                    if (variantCount >= 2)
                    {
                        variants.Add(new FoodVariant
                        {
                            Name = "ویژه",
                            Price = basePrice + (int)Math.Round(basePrice * 0.15),
                            FoodId = food.Id
                        });
                    }
                    if (variantCount == 3)
                    {
                        variants.Add(new FoodVariant
                        {
                            Name = "خانواده",
                            Price = basePrice + (int)Math.Round(basePrice * 0.30),
                            FoodId = food.Id
                        });
                    }

                    // Choose DEFAULT variant:
                    // • Prefer میان‌رده (special) if exists
                    // • Else highest price
                    var defaultVariant =
                        variants.FirstOrDefault(v => v.Name == "ویژه")
                        ?? variants.OrderByDescending(v => v.Price).First();

                    defaultVariant.IsDefault = true;

                    // Save variants
                    _db.FoodVariants.AddRange(variants);
                    await _db.SaveChangesAsync();

                    /* ---------------------------------------------------------
                       Addons SEEDING (per variant)
                       40% = no addons
                       30% = 1 addon
                       20% = 2 addons
                       10% = 3 addons
                    --------------------------------------------------------- */

                    foreach (var v in variants)
                    {
                        double addonRand = rand.NextDouble();
                        int addonsToCreate =
                            (addonRand < 0.40) ? 0 :
                            (addonRand < 0.70) ? 1 :
                            (addonRand < 0.90) ? 2 :
                                                 3;

                        // No addons → skip
                        if (addonsToCreate == 0)
                            continue;

                        for (int i = 0; i < addonsToCreate; i++)
                        {
                            var addon = new FoodAddon
                            {
                                FoodVariantId = v.Id,
                                Name = i switch
                                {
                                    0 => "پنیر اضافه",
                                    1 => "سس مخصوص",
                                    2 => "سیب‌زمینی کوچک",
                                    _ => "تاپینگ ویژه"
                                },
                                ExtraPrice = 8000 + rand.Next(0, 7000)
                            };

                            _db.FoodAddons.Add(addon);
                        }
                    }
                }
                await _db.SaveChangesAsync();


                /* ============================================================
                   Restaurant Discounts (new structure)
                   - Random subset of foods get discount
                   - Restaurants inherit max food discount (for ribbon)
                ============================================================ */
                var percentPool = new[] { 10, 15, 20, 25, 30 };
                var allRestaurants = await _db.Restaurants.Include(r => r.Foods).ToListAsync();

                foreach (var r in allRestaurants)
                {
                    if (!r.Foods.Any()) continue;

                    var discountedFoods = new List<int>();
                    foreach (var f in r.Foods)
                    {
                        if (rand.NextDouble() < 0.35) // ~35% of foods discounted
                        {
                            var percent = percentPool[rand.Next(percentPool.Length)];
                            _db.RestaurantDiscounts.Add(new RestaurantDiscount
                            {
                                RestaurantId = r.Id,
                                FoodId = f.Id,
                                Percent = percent,
                                StartDate = DateTime.UtcNow.AddDays(-rand.Next(0, 2)),
                                EndDate = DateTime.UtcNow.AddDays(rand.Next(7, 20))
                            });
                            discountedFoods.Add(percent);
                        }
                    }

                    // Ribbon logic — compute max discount
                    if (discountedFoods.Any())
                    {
                        int maxDiscount = discountedFoods.Max();
                        r.Description += $"{maxDiscount}%";
                    }
                }
                await _db.SaveChangesAsync();

                /* ============================================================
                   Ratings (restaurants & foods)
                ============================================================ */
                var allUsers = await _db.Users.ToListAsync();

                foreach (var r in allRestaurants)
                {
                    if (await _db.RestaurantRatings.AnyAsync(x => x.RestaurantId == r.Id)) continue;

                    int howMany = rand.Next(MinRestRatings, MaxRestRatings + 1);
                    var voters = allUsers.Where(u => u.Id != r.OwnerUserId)
                                         .OrderBy(_ => Guid.NewGuid())
                                         .Take(howMany)
                                         .ToList();

                    foreach (var user in voters)
                    {
                        _db.RestaurantRatings.Add(new RestaurantRating
                        {
                            RestaurantId = r.Id,
                            UserId = user.Id,
                            Score = rand.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 60))
                        });
                    }
                }

                var allFoodsFinal = await _db.Foods.ToListAsync();
                foreach (var food in allFoodsFinal)
                {
                    if (await _db.FoodRatings.AnyAsync(fr => fr.FoodId == food.Id)) continue;

                    int howMany = rand.Next(MinFoodRatings, MaxFoodRatings + 1);
                    var voters = allUsers.OrderBy(_ => Guid.NewGuid()).Take(howMany).ToList();

                    foreach (var user in voters)
                    {
                        _db.FoodRatings.Add(new FoodRating
                        {
                            FoodId = food.Id,
                            UserId = user.Id,
                            Score = rand.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 45))
                        });
                    }
                }
                await _db.SaveChangesAsync();

                /* ============================================================
                   Ad Banners
                ============================================================ */
                var now = DateTime.UtcNow;
                var bannerRestaurantIds = await _db.RestaurantAdBanners.Select(b => b.RestaurantId).ToListAsync();

                if (bannerRestaurantIds.Count < TargetAdBanners)
                {
                    var candidates = await _db.Restaurants
                        .Where(r => r.IsActive && r.IsApproved && !bannerRestaurantIds.Contains(r.Id))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(TargetAdBanners - bannerRestaurantIds.Count)
                        .Select(r => new { r.Id })
                        .ToListAsync();

                    var slogans = new[]
                    {
                        "قهوه‌هامون تازه‌برشتن ☕",
                        "پیتزاهای داغ با تخفیف 🍕",
                        "ساندویچ‌های خوشمزه دمِ‌دست 🥪",
                        "غذای خونگی با طعم نوستالژی 🍲",
                        "دسرهای ویژه امروز 🍮"
                    };

                    foreach (var c in candidates)
                    {
                        _db.RestaurantAdBanners.Add(new RestaurantAdBanner
                        {
                            RestaurantId = c.Id,
                            ImageUrl = BannerImages[rand.Next(BannerImages.Length)],
                            StartDate = now.AddDays(-1),
                            EndDate = now.AddDays(14),
                            CommercialText = slogans[rand.Next(slogans.Length)],
                            PurchasedViews = 600 + rand.Next(0, 900),
                            ConsumedViews = 0,
                            IsPaused = false
                        });
                    }
                    await _db.SaveChangesAsync();
                }

                /* ============================================================
                   Demo Customer + Orders
                ============================================================ */
                var demoPhone = "09121112233";
                var demoCustomer = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == demoPhone);

                if (demoCustomer == null)
                {
                    demoCustomer = new User
                    {
                        UserName = demoPhone,
                        PhoneNumber = demoPhone,
                        FullName = "مشتری نمونه"
                    };
                    await _userManager.CreateAsync(demoCustomer, "Customer123!");
                    await _userManager.AddToRoleAsync(demoCustomer, SD.Role_Customer);
                }

                if (!await _db.Orders.AnyAsync(o => o.UserId == demoCustomer.Id))
                {
                    var restaurantIds = await _db.Restaurants
                        .Where(r => r.IsActive && r.IsApproved)
                        .OrderBy(_ => Guid.NewGuid())
                        .Select(r => r.Id)
                        .Take(8)
                        .ToListAsync();

                    int dayOffset = 0;

                    foreach (var rid in restaurantIds)
                    {
                        var foods = await _db.Foods
                            .Where(f => f.RestaurantId == rid && f.IsAvailable && !f.IsDeleted)
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(rand.Next(2, 5))
                            .ToListAsync();

                        if (!foods.Any()) continue;

                        decimal totalAmount = 0m;
                        var orderItems = new List<OrderItem>();

                        foreach (var food in foods)
                        {
                            var quantity = rand.Next(1, 3);
                            var unitPrice = food.Price;
                            totalAmount += unitPrice * quantity;

                            orderItems.Add(new OrderItem
                            {
                                FoodId = food.Id,
                                Quantity = quantity,
                                UnitPrice = unitPrice
                            });
                        }

                        var order = new Order
                        {
                            UserId = demoCustomer.Id,
                            RestaurantId = rid,
                            Status = OrderStatus.Completed,
                            CreatedAt = DateTime.UtcNow.AddDays(-dayOffset++),
                            TotalAmount = totalAmount,
                            OrderItems = orderItems
                        };

                        _db.Orders.Add(order);
                    }
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeding error: {ex.Message}");
                throw;
            }
        }
    }
}
