using Menro.Application.Common.SD;
using Menro.Application.Extensions;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoRestaurantSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IRestaurantService _restaurantService;

    private readonly Random _rand = new(42);

    public DemoRestaurantSeeder(
        MenroDbContext db,
        UserManager<User> userManager,
        IRestaurantService restaurantService)
    {
        _db = db;
        _userManager = userManager;
        _restaurantService = restaurantService;
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

        var restaurants = new List<Restaurant>();

        for (int i = 1; i <= restaurantNames.Length; i++)
        {
            string email = $"owner{i}@menro.com";

            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            User owner;

            if (existingUser == null)
            {
                owner = new User
                {
                    UserName = $"0912{345678 + i}",
                    Email = email,
                    FullName = $"صاحب رستوران {i}",
                    PhoneNumber = $"0912{345678 + i}",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var createResult = await _userManager
                    .CreateAsync(owner, "Owner123!");

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ",
                        createResult.Errors.Select(x => x.Description));

                    throw new Exception(errors);
                }

                await _userManager.AddToRoleAsync(
                    owner,
                    SD.Role_Owner);
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

                ContactNumber = owner.PhoneNumber!,

                OpenTime = new TimeSpan(8 + i % 4, 0, 0),
                CloseTime = new TimeSpan(21, 0, 0),

                Description =
                    $"توضیح نمونه برای {name}",

                NationalCode = (1000000000 + i).ToString(),
                BankAccountNumber = (2000000000 + i).ToString(),
                ShebaNumber = $"IR{3000000000 + i}",

                OwnerUserId = owner.Id,

                RestaurantCategoryId = i % 8 + 1,

                CarouselImageUrl = "/img/res-slider.jpg",
                BannerImageUrl = "/img/res-card-1.png",
                ShopBannerImageUrl = "/img/ad-banner-1.jpg",
                LogoImageUrl = "/img/logo-orange.png",

                TableCount = _rand.Next(6, 20),

                Status = RestaurantStatus.Approved,

                IsActive = true,
                IsDeleted = false,

                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };

            restaurants.Add(restaurant);
        }

        await _db.Restaurants.AddRangeAsync(restaurants);

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

                        ImageUrl = "/img/drink.png",

                        IsAvailable = true,

                        CreatedAt =
                            DateTime.UtcNow.AddDays(
                                -_rand.Next(0, 30))
                    });
                }
            }
        }

        await _db.Foods.AddRangeAsync(foods);

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[Seed] {restaurants.Count} demo restaurants seeded.");
    }
}