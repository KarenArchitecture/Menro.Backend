using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Menro.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MenroDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        /* ---------- knobs you can tune (kept original values) ---------- */
        private const int RestaurantsToCreate = 12;           // total sample restaurants
        private const int MinCatsPerRestaurant = 4;           // categories per restaurant (restaurant-local)
        private const int MaxCatsPerRestaurant = 6;
        private const int MinFoodsPerCategory = 6;            // foods per (restaurant) category
        private const int MaxFoodsPerCategory = 9;
        private const int MinRestRatings = 3;                 // rating counts
        private const int MaxRestRatings = 7;
        private const int MinFoodRatings = 2;
        private const int MaxFoodRatings = 6;
        private const int TargetAdBanners = 5;                // how many restaurants should have a live ad banner

        // keep your original image placeholders
        private static readonly string[] BannerImages = new[] { "/img/top-banner.png", "/img/optcropban.jpg", "/img/res-slider.jpg" };
        private static readonly string[] CarouselImages = new[] { "/img/res-slider.jpg", "/img/optcropban.jpg" };
        private static readonly string[] CardImages = new[] { "/img/res-card-1.png", "/img/res-card-2.png" };
        private static readonly string[] Logos = new[] { "/img/logo-orange.png", "/img/logo-green.png" };
        private static readonly string FoodFallbackImage = "/img/drink.png"; // make sure this exists

        public DbInitializer(MenroDbContext db, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /* ============================================================
           Helpers
        ============================================================ */

        private static string Sluggify(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Trim().ToLower().Replace(" ", "-");
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseName)
        {
            var baseSlug = Sluggify(baseName);
            var slug = baseSlug;
            var i = 1;
            while (await _db.Restaurants.AnyAsync(r => r.Slug == slug))
            {
                slug = $"{baseSlug}-{i}";
                i++;
            }
            return slug;
        }

        // Seed Global Food Categories (uses your Menro.Infrastructure.Seed.GlobalFoodCategorySeed)
        private async Task SeedGlobalFoodCategoriesAsync()
        {
            if (await _db.GlobalFoodCategories.AnyAsync()) return;
            _db.GlobalFoodCategories.AddRange(GlobalFoodCategorySeed.Data);
            await _db.SaveChangesAsync();
        }

        // Food name pools for nicer variety (by global category)
        private static readonly Dictionary<string, string[]> FoodNamesByGlobal = new()
        {
            ["پیتزا"] = new[] { "پیتزا ناپلی", "پیتزا پپرونی", "پیتزا مارگاریتا", "پیتزا چهار فصل", "پیتزا قارچ و مرغ", "پیتزا ویژه", "پیتزا باربیکیو" },
            ["برگر"] = new[] { "چیزبرگر دوبل", "برگر کلاسیک", "اسموکی برگر", "برگر قارچ‌سوخاری", "چیکن برگر", "برگر ویژه منرو" },
            ["نوشیدنی گرم"] = new[] { "اسپرسو", "کاپوچینو", "لاته", "موکا", "آمریکانو", "هات چاکلت", "چای ماسالا" },
            ["نوشیدنی سرد"] = new[] { "موکتل بری", "لیموناد نعنایی", "موهیتو", "شیک وانیل", "شیک شکلات", "آیس لاته", "آیس آمریکانو" },
            ["سالاد"] = new[] { "سالاد سزار", "سالاد یونانی", "سالاد فصل", "سالاد کینوا", "سالاد مرغ گریل" },
            ["دسر"] = new[] { "براونی شکلاتی", "چیزکیک نیویورکی", "تیرا میسو", "پاناكوتا", "فرانچ‌توست کاراملی" }
        };

        private static (int min, int max) PriceRangeFor(string globalCat)
        {
            // Toman-ish ranges; tweak to your liking
            return globalCat switch
            {
                "پیتزا" => (280_000, 620_000),
                "برگر" => (220_000, 480_000),
                "نوشیدنی گرم" => (80_000, 180_000),
                "نوشیدنی سرد" => (90_000, 220_000),
                "سالاد" => (160_000, 340_000),
                "دسر" => (120_000, 260_000),
                _ => (100_000, 300_000)
            };
        }

        private static int NextPrice(Random rnd, (int min, int max) range)
        {
            return rnd.Next(range.min, range.max + 1);
        }

        public async Task InitializeAsync()
        {
            try
            {
                /* ============================================================
                   Migrate (if needed)
                ============================================================ */
                if (_db.Database.GetPendingMigrations().Any())
                    await _db.Database.MigrateAsync();

                /* ============================================================
                   Global Food Categories (admin-managed)
                ============================================================ */
                await SeedGlobalFoodCategoriesAsync();

                /* ============================================================
                   Roles
                ============================================================ */
                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                /* ============================================================
                   Admin user
                ============================================================ */
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

                // cached list of global categories (only active)
                var globalCats = await _db.GlobalFoodCategories
                    .Where(gc => gc.IsActive)
                    .OrderBy(gc => gc.DisplayOrder)
                    .ToListAsync();

                /* ============================================================
                   Owners + Restaurants + Restaurant-local Categories (mapped to Global)
                ============================================================ */

                // Realistic names pool (12 unique names)
                var restNames = new[]
                {
                    "پیتزا بامبو",
                    "کافه مانا",
                    "برگرستان",
                    "رستوران نوفل‌لوشاتو",
                    "کافه چرخ",
                    "پاستا کونتو",
                    "سوشی یو",
                    "دلمه خانه",
                    "کباب‌سرای پارس",
                    "کترینگ سیب",
                    "نان و نمک",
                    "شیرینی‌سرای گل"
                };

                for (int i = 1; i <= RestaurantsToCreate; i++)
                {
                    string email = $"owner{i}@menro.com";
                    if (await _db.Users.AnyAsync(u => u.Email == email)) continue;

                    // create owner account
                    var owner = new User
                    {
                        UserName = $"0912{345678 + i}",
                        Email = email,
                        FullName = $"صاحب رستوران {i}",
                        PhoneNumber = $"0912{345678 + i}"
                    };
                    await _userManager.CreateAsync(owner, "Owner123!");
                    await _userManager.AddToRoleAsync(owner, SD.Role_Owner);

                    // choose a realistic name (cyclic)
                    var restName = restNames[(i - 1) % restNames.Length];
                    var slug = await GenerateUniqueSlugAsync(restName + "-" + i); // ensure uniqueness

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
                       Special custom categories (always CustomFoodCategory)
                       - they reuse SvgIcon from GlobalFoodCategory (but do NOT reference it by FK)
                    ------------------------- */
                    var specialCategoryNames = new[] { "پیشنهاد سرآشپز", "پرفروش‌ترین‌ها", "ویژه امروز" };
                    foreach (var specialName in specialCategoryNames)
                    {
                        var selectedGlobalCat = globalCats[rand.Next(globalCats.Count)];

                        var specialCat = new CustomFoodCategory
                        {
                            Name = specialName,
                            SvgIcon = selectedGlobalCat.SvgIcon, // re-use admin-uploaded icon
                            RestaurantId = restaurant.Id
                            // <-- NO GlobalFoodCategoryId (by design)
                        };
                        _db.CustomFoodCategory.Add(specialCat);
                        await _db.SaveChangesAsync();

                        // Add 2-3 foods to this special category
                        var count = rand.Next(2, 4);
                        for (int f = 0; f < count; f++)
                        {
                            _db.Foods.Add(new Food
                            {
                                Name = $"{specialName} {f + 1}",
                                Ingredients = "مواد اولیه تازه و با کیفیت",
                                Price = rand.Next(150_000, 400_000),
                                CustomFoodCategoryId = specialCat.Id, // note: FoodCategoryId (nullable int) -> CustomFoodCategory
                                RestaurantId = restaurant.Id,
                                ImageUrl = FoodFallbackImage,
                                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 30)),
                                IsAvailable = true
                            });
                        }
                        await _db.SaveChangesAsync();
                    }

                    /* -------------------------
                       Regular restaurant categories (mix of Global-mapped or Custom)
                    ------------------------- */
                    var catCount = rand.Next(MinCatsPerRestaurant, MaxCatsPerRestaurant + 1);
                    for (int c = 0; c < catCount; c++)
                    {
                        bool useGlobal = rand.NextDouble() < 0.5;
                        if (useGlobal && globalCats.Any())
                        {
                            var globalCat = globalCats[rand.Next(globalCats.Count)];
                            int foodCount = rand.Next(MinFoodsPerCategory, MaxFoodsPerCategory + 1);
                            var pool = FoodNamesByGlobal.TryGetValue(globalCat.Name, out var arr) ? arr : new[] { "آیتم ویژه", "آیتم محبوب" };

                            for (int k = 0; k < foodCount; k++)
                            {
                                _db.Foods.Add(new Food
                                {
                                    Name = pool[k % pool.Length],
                                    Ingredients = "مواد اولیه تازه و با کیفیت",
                                    Price = NextPrice(rand, PriceRangeFor(globalCat.Name)),
                                    RestaurantId = restaurant.Id,
                                    GlobalFoodCategoryId = globalCat.Id, // mapped to global
                                    ImageUrl = FoodFallbackImage,
                                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 45)),
                                    IsAvailable = true
                                });
                            }
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            var customCat = new CustomFoodCategory
                            {
                                Name = $"دسته {c + 1}",
                                SvgIcon = globalCats[rand.Next(globalCats.Count)].SvgIcon,
                                RestaurantId = restaurant.Id
                                // <-- NO GlobalFoodCategoryId
                            };
                            _db.CustomFoodCategory.Add(customCat);
                            await _db.SaveChangesAsync();

                            int foodCount = rand.Next(MinFoodsPerCategory, MaxFoodsPerCategory + 1);
                            //for (int k = 0; k < foodCount; k++)
                            //{
                            //    _db.Foods.Add(new Food
                            //    {
                            //        Name = $"آیتم {c + 1}-{k + 1}",
                            //        Ingredients = "مواد اولیه تازه و با کیفیت",
                            //        Price = rand.Next(150_000, 400_000),
                            //        CustomFoodCategoryId = customCat.Id,
                            //        RestaurantId = restaurant.Id,
                            //        ImageUrl = FoodFallbackImage,
                            //        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 45)),
                            //        IsAvailable = true
                            //    });
                            //}
                            await _db.SaveChangesAsync();
                        }
                    }
                } // end restaurants loop

                /* ============================================================
                   Variants + Addons seeding (enhancement)
                ============================================================ */
                var seededFoods = await _db.Foods.ToListAsync();
                foreach (var food in seededFoods)
                {
                    if (rand.NextDouble() < 0.55)
                    {
                        var basePrice = Math.Max(5000, food.Price);
                        var variants = new List<FoodVariant>();

                        variants.Add(new FoodVariant
                        {
                            Name = "کوچک",
                            Price = Math.Max(0, basePrice - (int)Math.Round(basePrice * 0.15)),
                            FoodId = food.Id
                        });

                        variants.Add(new FoodVariant
                        {
                            Name = "متوسط",
                            Price = basePrice,
                            FoodId = food.Id
                        });

                        if (rand.NextDouble() < 0.6)
                        {
                            variants.Add(new FoodVariant
                            {
                                Name = "بزرگ",
                                Price = basePrice + (int)Math.Round(basePrice * 0.2),
                                FoodId = food.Id
                            });
                        }

                        _db.FoodVariants.AddRange(variants);
                        await _db.SaveChangesAsync(); // to get variant IDs

                        foreach (var v in variants)
                        {
                            int addonCount = rand.Next(0, 4);
                            for (int a = 0; a < addonCount; a++)
                            {
                                var addon = new FoodAddon
                                {
                                    Name = a switch
                                    {
                                        0 => "پنیر اضافه",
                                        1 => "سس ویژه",
                                        2 => "سیب زمینی کوچک",
                                        _ => "تاپینگ اختصاصی"
                                    },
                                    ExtraPrice = 8000 + rand.Next(0, 7000),
                                    FoodVariantId = v.Id
                                };
                                _db.FoodAddons.Add(addon);
                            }
                        }
                        await _db.SaveChangesAsync();
                    }
                }

                /* ============================================================
                   Restaurant Discounts
                ============================================================ */
                var restaurantsWithFoods = await _db.Restaurants
                    .Include(r => r.Foods)
                    .ToListAsync();

                var percentPool = new[] { 10, 15, 20, 25, 30 };
                foreach (var r in restaurantsWithFoods)
                {
                    if (rand.NextDouble() < 0.45) continue;

                    if (await _db.RestaurantDiscounts.AnyAsync(d => d.RestaurantId == r.Id))
                        continue;

                    int howMany = rand.Next(1, 3); // 1..2 discounts
                    var foodIds = r.Foods.Select(f => f.Id).ToList();

                    for (int k = 0; k < howMany; k++)
                    {
                        var percent = percentPool[rand.Next(percentPool.Length)];
                        int? foodId = null;
                        if (foodIds.Count > 0 && rand.NextDouble() < 0.8)
                            foodId = foodIds[rand.Next(foodIds.Count)];

                        _db.RestaurantDiscounts.Add(new RestaurantDiscount
                        {
                            RestaurantId = r.Id,
                            FoodId = foodId,
                            Percent = percent,
                            StartDate = DateTime.UtcNow.AddDays(-rand.Next(0, 2)),
                            EndDate = DateTime.UtcNow.AddDays(rand.Next(7, 20))
                        });
                    }
                }
                await _db.SaveChangesAsync();

                /* ============================================================
                   Ratings (restaurants & foods)
                ============================================================ */
                var allUsers = await _db.Users.ToListAsync();
                var allRestaurants = await _db.Restaurants.ToListAsync();

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
                            Score = rand.Next(3, 6), // 3..5
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 60))
                        });
                    }
                }
                await _db.SaveChangesAsync();

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

                var bannerRestaurantIds = await _db.RestaurantAdBanners
                    .Select(b => b.RestaurantId)
                    .ToListAsync();

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
    Demo Customer + Orders (+ OrderItems!)
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

                // Only seed orders if none exist yet
                if (!await _db.Orders.AnyAsync(o => o.UserId == demoCustomer.Id))
                {
                    // Fetch active restaurants
                    var restaurantIds = await _db.Restaurants
                        .Where(r => r.IsActive && r.IsApproved)
                        .OrderBy(_ => Guid.NewGuid())
                        .Select(r => r.Id)
                        .Take(8)
                        .ToListAsync();

                    int dayOffset = 0;

                    foreach (var rid in restaurantIds)
                    {
                        // Fetch available foods only for this restaurant
                        var foods = await _db.Foods
                            .Where(f => f.RestaurantId == rid && f.IsAvailable && !f.IsDeleted)
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(rand.Next(2, 5)) // 2–4 items per order
                            .ToListAsync();

                        if (!foods.Any()) continue; // skip if restaurant has no foods

                        decimal totalAmount = 0m;
                        var orderItems = new List<OrderItem>();

                        foreach (var food in foods)
                        {
                            var quantity = rand.Next(1, 3); // 1–2 quantity
                            var unitPrice = food.Price;
                            var itemTotal = unitPrice * quantity;

                            totalAmount += itemTotal;

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
        } // end InitializeAsync
    }
}