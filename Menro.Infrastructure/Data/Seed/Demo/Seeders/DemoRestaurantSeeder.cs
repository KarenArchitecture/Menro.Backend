using Menro.Application.Common.SD;
using Menro.Application.Extensions;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Common.Helpers;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoRestaurantSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IRestaurantService _restaurantService;
    private readonly IMediaStorageProvider _mediaStorage;
    private readonly MediaStorageOptions _mediaOptions;

    private readonly Random _rand = new(42);

    public DemoRestaurantSeeder(
        MenroDbContext db,
        UserManager<User> userManager,
        IRestaurantService restaurantService,
        IMediaStorageProvider mediaStorage,
        IOptions<MediaStorageOptions> mediaOptions)
    {
        _db = db;
        _userManager = userManager;
        _restaurantService = restaurantService;
        _mediaStorage = mediaStorage;
        _mediaOptions = mediaOptions.Value;
    }
    public int Order => SeedOrder.Restaurant;
    public async Task SeedAsync()
    {
        var demoOwnerExists = await _userManager.Users
            .AnyAsync(x => x.Email == "owner1@menro.com");

        if (demoOwnerExists)
        {
            Console.WriteLine("[Seed] Demo restaurants already seeded.");
            return;
        }

        // 🔧 Sample raw images already sitting flat under wwwroot/media from
        // before the entity-scoped media system existed. We read them once
        // here and re-save them through the REAL media pipeline
        // (SaveBytesAsync) per-entity below, so every restaurant/food ends up
        // with a properly structured {entityId}/original|resized|thumbnail
        // set of webp files that GetUrl() can actually find — in dev AND in
        // production, since these source files ship with wwwroot either way.
        var logoBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/restaurant/logo/logo-green.png"));
        var bannerBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/restaurant/home/res-card-2.png"));
        var shopBannerBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/restaurant/shop/ad-banner-2.png"));
        var foodBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/food/drink.png"));

        var globalCats = await _db.GlobalFoodCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        var restaurantNames = new[]
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

        var tableLabels = new[]
        {
            "میز ۱",
            "میز ۲",
            "میز ۳",
            "میز خانواده ۱",
            "میز خانواده ۲",
            "میز کنار پنجره",
            "میز وی‌آی‌پی ۱",
            "میز تراس ۱",
            "میز تراس ۲"
        };

        var restaurants = new List<Restaurant>();

        for (int i = 1; i <= restaurantNames.Length; i++)
        {
            string email = $"owner{i}@menro.com";

            // 🔧 FIX: this must be LOCAL format (09...) — it used to be built
            // as "+98912100{i:D4}" (already "+98..."), which then got run
            // through ToE164() a second time and corrupted into
            // "+9898912100xxxx". PhoneNumberHelper.ToStorageFormat expects/
            // accepts local format too, but building it as local here keeps
            // this consistent with rawPhone's own doc-comment below and with
            // every other demo seeder (DemoCustomerSeeder, DemoOrderSeeder).
            var rawPhone = $"0912100{i:D4}"; // e.g. 09121000001 .. 09121000012 (11 digits)
            var storagePhone = PhoneNumberHelper.ToStorageFormat(rawPhone);

            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            User owner;


            if (existingUser == null)
            {
                owner = new User
                {
                    UserName = rawPhone,
                    Email = email,
                    FullName = $"صاحب رستوران {i}",
                    PhoneNumber = storagePhone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(owner, "Owner123!");

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                    throw new Exception(errors);
                }

                await _userManager.AddToRoleAsync(owner, SD.Role_Owner);
            }
            else
            {
                owner = existingUser;
            }

            var name = restaurantNames[i - 1];

            var slug = await _restaurantService
                .GenerateUniqueSlugAsync(
                    name.TransliterateToEnglish());

            var restaurant = new Restaurant
            {
                Name = name,
                Slug = slug,

                Address = $"تهران، خیابان نمونه {i}",

                // 🔧 stored in the same canonical +98 format as PhoneNumber —
                // keeps every phone-like field in the DB consistent; convert
                // to 09... only at the client boundary (via PhoneNumberHelper.ToClientFormat)
                // whenever this is returned in an API response.
                ContactNumber = storagePhone,

                OpenTime = new TimeSpan(8 + i % 4, 0, 0),
                CloseTime = new TimeSpan(21, 0, 0),

                Description =
                    $"توضیح نمونه برای {name}",

                NationalCode = (1000000000 + i).ToString(),
                BankAccountNumber = (2000000000 + i).ToString(),
                ShebaNumber = $"IR{3000000000 + i}",

                OwnerUserId = owner.Id,

                RestaurantCategoryId = i % 8 + 1,

                // 🔧 CarouselImageUrl intentionally left unset — it doesn't map
                // to any MediaCategory anywhere in the codebase yet. Flagged
                // for follow-up once its actual usage is confirmed.

                Status = RestaurantStatus.Approved,

                IsActive = true,
                IsDeleted = false,

                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };

            int tableCount = _rand.Next(6, 20);

            for (int t = 1; t <= tableCount; t++)
            {
                string label = t <= tableLabels.Length
                    ? tableLabels[t - 1]
                    : $"میز {t}";

                restaurant.Tables.Add(new RestaurantTable
                {
                    Label = label
                });
            }

            restaurants.Add(restaurant);
        }

        await _db.Restaurants.AddRangeAsync(restaurants);
        await _db.SaveChangesAsync(); // assigns restaurant.Id — needed as entityId below

        // 🔧 Now that every restaurant has a real Id, save its logo/banner/shop
        // banner through the actual media pipeline (per-entity webp variants).
        foreach (var restaurant in restaurants)
        {
            var entityId = restaurant.Id.ToString();

            var logoResult = await _mediaStorage.SaveBytesAsync(MediaCategory.RestaurantLogo, logoBytes, ".png", entityId);
            restaurant.LogoImageUrl = logoResult.FileName;

            var bannerResult = await _mediaStorage.SaveBytesAsync(MediaCategory.RestaurantHomeBanner, bannerBytes, ".png", entityId);
            restaurant.BannerImageUrl = bannerResult.FileName;

            var shopBannerResult = await _mediaStorage.SaveBytesAsync(MediaCategory.RestaurantShopBanner, shopBannerBytes, ".png", entityId);
            restaurant.ShopBannerImageUrl = shopBannerResult.FileName;
        }
        await _db.SaveChangesAsync();

        var foods = new List<Food>();
        var customCategories = new List<CustomFoodCategory>();

        foreach (var restaurant in restaurants)
        {
            var selectedCats = globalCats
                .OrderBy(_ => Guid.NewGuid())
                .Take(_rand.Next(4, 7))
                .ToList();

            foreach (var globalCat in selectedCats)
            {
                var customCat = new CustomFoodCategory
                {
                    Name = globalCat.Name,
                    IconId = globalCat.IconId,
                    RestaurantId = restaurant.Id,
                    GlobalCategoryId = globalCat.Id
                };

                customCategories.Add(customCat);

                _db.CustomFoodCategories.Add(customCat);

                await _db.SaveChangesAsync();

                int foodCount = _rand.Next(6, 10);

                for (int i = 1; i <= foodCount; i++)
                {
                    foods.Add(new Food
                    {
                        Name = $"{globalCat.Name} {i}",

                        Ingredients =
                            "مواد اولیه تازه و باکیفیت",

                        Price = _rand.Next(80_000, 500_000),

                        RestaurantId = restaurant.Id,

                        CustomFoodCategoryId = customCat.Id,

                        IsAvailable = true,

                        CreatedAt =
                            DateTime.UtcNow.AddDays(
                                -_rand.Next(0, 30))
                    });
                }
            }
        }

        await _db.Foods.AddRangeAsync(foods);
        await _db.SaveChangesAsync(); // assigns food.Id — needed as entityId below

        // 🔧 Same idea for every seeded food's image.
        foreach (var food in foods)
        {
            var foodImgResult = await _mediaStorage.SaveBytesAsync(MediaCategory.RestaurantFoodImage, foodBytes, ".png", food.Id.ToString());
            food.ImageUrl = foodImgResult.FileName;
        }
        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[Seed] {restaurants.Count} demo restaurants seeded.");
    }
}