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

        private static readonly string[] BannerImages = { "top-banner.png", "optcropban.jpg", "res-slider.jpg" };
        private static readonly string[] CarouselImages = { "res-slider.jpg", "optcropban.jpg" };
        private static readonly string[] CardImages = { "res-card-1.png", "res-card-2.png" };
        private static readonly string[] ShopBannerImages = { "ad-banner-1.jpg", "ad-banner-2.png" };
        private static readonly string[] Logos = { "logo-orange.png", "logo-green.png" };
        private static readonly string FoodFallbackImage = "drink.png";

        // ✅ Used by RestaurantAd.ImageFileName
        // These should exist where BuildAdImageUrl expects ad images.
        private static readonly string[] CarouselAdImages =
        {
            "res-slider.jpg",
            "optcropban.jpg",
            "top-banner.png"
        };

        private static readonly string[] FullscreenAdImages =
        {
            "ad-banner-1.jpg",
            "ad-banner-2.png",
            "top-banner.png"
        };

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

        private async Task SeedIconsAsync()
        {
            if (await _db.Icons.AnyAsync()) return;

            _db.Icons.AddRange(IconSeed.Data);
            await _db.SaveChangesAsync();
        }

        private async Task SeedGlobalFoodCategoriesAsync()
        {
            if (await _db.GlobalFoodCategories.AnyAsync()) return;

            _db.GlobalFoodCategories.AddRange(GlobalFoodCategorySeed.Data);
            await _db.SaveChangesAsync();
        }

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

        private async Task SeedRestaurantAdsAsync(Random rand)
        {
            // Prevent duplicate ads when initializer runs multiple times
            if (await _db.RestaurantAds.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            var restaurants = await _db.Restaurants
                .Where(r =>
                    r.IsActive &&
                    !r.IsDeleted &&
                    r.Status == RestaurantStatus.Approved)
                .OrderBy(r => r.Id)
                .Take(10)
                .ToListAsync();

            if (!restaurants.Any())
                return;

            var carouselTexts = new[]
            {
                "افتتاحیه ویژه؛ تجربه‌ای تازه از طعم",
                "پیشنهاد ویژه امروز فقط در منرو",
                "غذاهای محبوب با ارسال سریع",
                "طعم متفاوت، انتخاب هوشمندانه",
                "رستوران منتخب این هفته"
            };

            var bannerTexts = new[]
            {
                "تخفیف ویژه سفارش آنلاین",
                "پیشنهاد امروز را از دست ندهید",
                "ارسال سریع، غذای تازه",
                "طعم محبوب کاربران منرو",
                "ویژه مشتریان منرو"
            };

            /* ============================================================
               Main Slider Ads
               Used by:
               GET /api/public/restaurant/featured
            ============================================================ */

            var carouselCount = Math.Min(5, restaurants.Count);

            for (int i = 0; i < carouselCount; i++)
            {
                var restaurant = restaurants[i];

                var billingType = i % 2 == 0
                    ? AdBillingType.PerDay
                    : AdBillingType.PerClick;

                var purchasedUnits = billingType == AdBillingType.PerDay
                    ? rand.Next(7, 15)
                    : rand.Next(500, 1500);

                var ad = new RestaurantAd
                {
                    RestaurantId = restaurant.Id,

                    PlacementType = AdPlacementType.MainSlider,
                    BillingType = billingType,

                    ImageFileName = CarouselAdImages[i % CarouselAdImages.Length],
                    TargetUrl = restaurant.Slug,
                    CommercialText = carouselTexts[i % carouselTexts.Length],

                    CreatedAt = now.AddDays(-rand.Next(1, 5)),
                    StartDate = now.AddDays(-1),

                    EndDate = billingType == AdBillingType.PerDay
                        ? now.AddDays(purchasedUnits)
                        : now.AddMonths(3),

                    PurchasedUnits = purchasedUnits,
                    ConsumedUnits = 0,

                    Cost = billingType == AdBillingType.PerDay
                        ? purchasedUnits * 250_000
                        : purchasedUnits * 2_000,

                    Status = AdStatus.Approved,
                    AdminNotes = null
                };

                _db.RestaurantAds.Add(ad);
            }

            /* ============================================================
               Fullscreen Banner Ads
               Used by:
               GET /api/public/restaurant/ad-banner/random
            ============================================================ */

            var bannerRestaurants = restaurants
                .OrderBy(_ => Guid.NewGuid())
                .Take(TargetAdBanners)
                .ToList();

            for (int i = 0; i < bannerRestaurants.Count; i++)
            {
                var restaurant = bannerRestaurants[i];

                var billingType = i % 2 == 0
                    ? AdBillingType.PerView
                    : AdBillingType.PerClick;

                var purchasedUnits = billingType == AdBillingType.PerView
                    ? rand.Next(3_000, 8_000)
                    : rand.Next(400, 1200);

                var ad = new RestaurantAd
                {
                    RestaurantId = restaurant.Id,

                    PlacementType = AdPlacementType.FullscreenBanner,
                    BillingType = billingType,

                    ImageFileName = FullscreenAdImages[i % FullscreenAdImages.Length],
                    TargetUrl = restaurant.Slug,
                    CommercialText = bannerTexts[i % bannerTexts.Length],

                    CreatedAt = now.AddDays(-rand.Next(1, 5)),
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddMonths(3),

                    PurchasedUnits = purchasedUnits,
                    ConsumedUnits = 0,

                    Cost = billingType == AdBillingType.PerView
                        ? purchasedUnits * 500
                        : purchasedUnits * 2_500,

                    Status = AdStatus.Approved,
                    AdminNotes = null
                };

                _db.RestaurantAds.Add(ad);
            }

            await _db.SaveChangesAsync();
        }

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
                   Core Seeds: Icons + Global Categories + Roles + Admin
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

                    if (await _db.Users.AnyAsync(u => u.Email == email))
                        continue;

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
                    var slug = await _restaurantService.GenerateUniqueSlugAsync(
                        restName.TransliterateToEnglish());

                    var restaurant = new Restaurant
                    {
                        Name = restName,
                        Address = $"تهران، خیابان نمونه {i}",
                        ContactNumber = owner.PhoneNumber ?? $"0912{345678 + i}",

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
                        ShopBannerImageUrl = ShopBannerImages[(i - 1) % ShopBannerImages.Length],
                        LogoImageUrl = Logos[(i - 1) % Logos.Length],

                        TableCount = rand.Next(6, 21),

                        IsActive = true,
                        IsDeleted = false,
                        Status = RestaurantStatus.Approved,

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
                       Restaurant Categories
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

                        while (await _db.CustomFoodCategories.AnyAsync(x =>
                            x.RestaurantId == restaurant.Id &&
                            x.Name == customCat.Name))
                        {
                            duplicateCounter++;
                            customCat.Name = $"{baseName} {duplicateCounter}";
                        }

                        _db.CustomFoodCategories.Add(customCat);
                        await _db.SaveChangesAsync();

                        int foodCount = rand.Next(MinFoodsPerCategory, MaxFoodsPerCategory + 1);

                        var pool = customCat.GlobalCategoryId.HasValue &&
                                   FoodNamesByGlobal.TryGetValue(customCat.Name, out var arr)
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
                }

                /* ============================================================
                   Variants + Addons
                ============================================================ */
                var seededFoods = await _db.Foods
                    .Include(f => f.Variants)
                    .ThenInclude(v => v.Addons)
                    .ToListAsync();

                foreach (var food in seededFoods)
                {
                    if (food.Variants != null && food.Variants.Any())
                        continue;

                    double r = rand.NextDouble();

                    int variantCount =
                        (r < 0.30) ? 0 :
                        (r < 0.50) ? 1 :
                        (r < 0.80) ? 2 :
                                      3;

                    if (variantCount == 0)
                        continue;

                    var basePrice = Math.Max(5000, food.Price);
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

                    var defaultVariant =
                        variants.FirstOrDefault(v => v.Name == "ویژه")
                        ?? variants.OrderByDescending(v => v.Price).First();

                    defaultVariant.IsDefault = true;

                    _db.FoodVariants.AddRange(variants);
                    await _db.SaveChangesAsync();

                    foreach (var v in variants)
                    {
                        double addonRand = rand.NextDouble();

                        int addonsToCreate =
                            (addonRand < 0.40) ? 0 :
                            (addonRand < 0.70) ? 1 :
                            (addonRand < 0.90) ? 2 :
                                                 3;

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
   Restaurant Discounts (FINAL VERSION)
============================================================ */

                var percentPool = new[] { 10m, 15m, 20m, 25m, 30m };

                var allRestaurants = await _db.Restaurants
                    .Include(x => x.Foods)
                    .ToListAsync();

                foreach (var rr in allRestaurants)
                {
                    if (!rr.Foods.Any())
                        continue;

                    // =========================================================
                    // STEP 1: Only SOME restaurants get discounts
                    // =========================================================
                    bool hasDiscount = rand.NextDouble() < 0.35; // 35% only

                    if (!hasDiscount)
                        continue;

                    decimal? maxDiscount = null;

                    // =========================================================
                    // STEP 2: Pick limited foods only (not whole menu)
                    // =========================================================
                    var discountedFoods = rr.Foods
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(rand.Next(1, Math.Min(4, rr.Foods.Count))) // 1–3 foods usually
                        .ToList();

                    foreach (var f in discountedFoods)
                    {
                        // Not every selected food gets discount (adds realism)
                        if (rand.NextDouble() < 0.5)
                        {
                            var percent = percentPool[rand.Next(percentPool.Length)];

                            var discount = new Discount
                            {
                                Scope = DiscountScope.Food,
                                RestaurantId = rr.Id,
                                FoodId = f.Id,

                                ValueType = DiscountValueType.Percent,
                                Value = percent,

                                StartDate = DateTime.UtcNow.AddDays(-rand.Next(0, 3)),
                                EndDate = DateTime.UtcNow.AddDays(rand.Next(5, 15)),

                                IsActive = true,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            _db.Discounts.Add(discount);

                            if (!maxDiscount.HasValue || percent > maxDiscount.Value)
                                maxDiscount = percent;
                        }
                    }

                    // =========================================================
                    // STEP 3: Only show meaningful max discount in description
                    // =========================================================
                    if (maxDiscount.HasValue && maxDiscount.Value >= 10)
                    {
                        rr.Description += $" 🔥 تا {maxDiscount.Value}% تخفیف";
                    }
                }

                /* ============================================================
                   Ratings
                ============================================================ */
                var allUsers = await _db.Users.ToListAsync();

                foreach (var rr in allRestaurants)
                {
                    if (await _db.RestaurantRatings.AnyAsync(x => x.RestaurantId == rr.Id))
                        continue;

                    int howMany = rand.Next(MinRestRatings, MaxRestRatings + 1);

                    var voters = allUsers
                        .Where(u => u.Id != rr.OwnerUserId)
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(howMany)
                        .ToList();

                    foreach (var user in voters)
                    {
                        _db.RestaurantRatings.Add(new RestaurantRating
                        {
                            RestaurantId = rr.Id,
                            UserId = user.Id,
                            Score = rand.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 60))
                        });
                    }
                }

                var allFoodsFinal = await _db.Foods.ToListAsync();

                foreach (var food in allFoodsFinal)
                {
                    if (await _db.FoodRatings.AnyAsync(fr => fr.FoodId == food.Id))
                        continue;

                    int howMany = rand.Next(MinFoodRatings, MaxFoodRatings + 1);

                    var voters = allUsers
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(howMany)
                        .ToList();

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
                   Restaurant Ads: Carousel + Random Ad Banners
                ============================================================ */
                await SeedRestaurantAdsAsync(rand);

                /* ============================================================
                   Demo Customer + Orders
                ============================================================ */
                var demoPhone = "09121112233";
                var demoCustomer = await _db.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == demoPhone);

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
                        var rid = info.Id;

                        var foods = await _db.Foods
                            .Where(f =>
                                f.RestaurantId == rid &&
                                f.IsAvailable &&
                                !f.IsDeleted)
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(rand.Next(2, 5))
                            .ToListAsync();

                        if (!foods.Any())
                            continue;

                        decimal totalAmount = 0m;
                        var orderItems = new List<OrderItem>();

                        int? tableNumber;

                        if (rand.NextDouble() < 0.30 || info.TableCount <= 0)
                        {
                            tableNumber = null;
                        }
                        else
                        {
                            tableNumber = rand.Next(1, info.TableCount + 1);
                        }

                        foreach (var food in foods)
                        {
                            int quantity = rand.Next(1, 3);

                            var variantsForFood = allVariants
                                .Where(v => v.FoodId == food.Id)
                                .ToList();

                            if (variantsForFood.Count == 0)
                            {
                                decimal unitPrice = food.Price;
                                totalAmount += unitPrice * quantity;

                                var simpleItem = new OrderItem
                                {
                                    FoodId = food.Id,
                                    Quantity = quantity,
                                    UnitPrice = unitPrice,
                                    TitleSnapshot = food.Name
                                };

                                orderItems.Add(simpleItem);
                                continue;
                            }

                            var chosenVariant = variantsForFood
                                .FirstOrDefault(v => v.IsDefault == true)
                                ?? variantsForFood.OrderBy(_ => Guid.NewGuid()).First();

                            var variantAddons = chosenVariant.Addons?.ToList() ?? new List<FoodAddon>();
                            var selectedAddons = new List<FoodAddon>();

                            foreach (var addon in variantAddons)
                            {
                                if (rand.NextDouble() < 0.45)
                                    selectedAddons.Add(addon);
                            }

                            int addonsSum = selectedAddons.Sum(a => a.ExtraPrice);
                            decimal finalUnitPrice = chosenVariant.Price + addonsSum;

                            totalAmount += finalUnitPrice * quantity;

                            var orderItem = new OrderItem
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
                            };

                            orderItems.Add(orderItem);
                        }

                        var lastNumber = await _db.Orders
                            .Where(o => o.RestaurantId == rid)
                            .Select(o => (int?)o.RestaurantOrderNumber)
                            .MaxAsync() ?? 0;

                        var order = new Order
                        {
                            UserId = demoCustomer.Id,
                            RestaurantId = rid,
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