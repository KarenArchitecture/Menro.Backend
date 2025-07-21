using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MenroDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public DbInitializer(
            MenroDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // ✅ Simple English slug generator
        private string GenerateSlug(string persianName, int index)
        {
            return $"restaurant-number-{index}";
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                    _db.Database.Migrate();

                // 1️⃣ Roles
                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                // 2️⃣ Admin account
                if (!await _db.Users.AnyAsync(u => u.Email == "MenroAdmin@gmail.com"))
                {
                    var admin = new User
                    {
                        UserName = "MenroAdmin_1",
                        Email = "MenroAdmin@gmail.com",
                        FullName = "Admin",
                        PhoneNumber = "+989486813486"
                    };
                    await _userManager.CreateAsync(admin, "@Admin123456");
                    await _userManager.AddToRoleAsync(admin, SD.Role_Admin);
                }

                // 3️⃣ Seed 15 owners + restaurants
                for (int i = 1; i <= 15; i++)
                {
                    string email = $"owner{i}@menro.com";
                    if (await _db.Users.AnyAsync(u => u.Email == email)) continue;

                    var owner = new User
                    {
                        UserName = $"+98912{345678 + i}",
                        Email = email,
                        FullName = $"رستوران‌دار {i}",
                        PhoneNumber = $"+98912{345678 + i}"
                    };
                    await _userManager.CreateAsync(owner, "Owner123!");
                    await _userManager.AddToRoleAsync(owner, SD.Role_Owner);

                    // 4️⃣ Restaurant
                    var rest = new Restaurant
                    {
                        Name = $"رستوران شماره {i}",
                        Address = $"تهران، خیابان نمونه شماره {i}",
                        OpenTime = new TimeSpan(8 + (i % 5), 0, 0),
                        CloseTime = new TimeSpan(20 + (i % 4), 0, 0),
                        Description = $"توضیح نمونه برای رستوران {i}",
                        NationalCode = (1000000000 + i).ToString(),
                        BankAccountNumber = (2000000000 + i).ToString(),
                        ShebaNumber = $"IR{3000000000 + i}",
                        OwnerUserId = owner.Id,
                        RestaurantCategoryId = (i % 8) + 1,
                        CarouselImageUrl = $"/img/res-slider.png",
                        BannerImageUrl = $"/img/res-cards.png",
                        IsFeatured = (i % 3 == 0),
                        IsActive = true,
                        IsApproved = true,
                        Slug = GenerateSlug($"رستوران شماره {i}", i) // ✅ Added slug here
                        CreatedAt = DateTime.Now.AddDays(-i) // فرض: هر رستوران چند روز قبل‌تر ساخته شده
                    };

                    _db.Restaurants.Add(rest);
                    await _db.SaveChangesAsync(); // get ID

                    // 5️⃣ Food categories
                    var cats = new List<FoodCategory>();
                    for (int j = 1; j <= 5; j++)
                        cats.Add(new FoodCategory { Name = $"دسته {j}", RestaurantId = rest.Id });

                    _db.FoodCategories.AddRange(cats);
                }

                await _db.SaveChangesAsync();

                // 6️⃣ Seed Food items for each FoodCategory
                var allFoodCategories = await _db.FoodCategories.Include(fc => fc.Restaurant).ToListAsync();
                var allUsersForRating = await _db.Users.ToListAsync();

                var random = new Random();

                var foods = new List<Food>();
                foreach (var category in allFoodCategories)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        foods.Add(new Food
                        {
                            Name = $"غذای نمونه {i} دسته {category.Name}",
                            Ingredients = "مواد اولیه نمونه",
                            Price = random.Next(15000, 80000),
                            FoodCategoryId = category.Id,
                            RestaurantId = category.RestaurantId,
                            ImageUrl = "/img/drink.png",
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                        });
                    }
                }

                _db.Foods.AddRange(foods);
                await _db.SaveChangesAsync();

                // 7️⃣ Seed FoodRatings
                var foodRatings = new List<FoodRating>();
                var existingFoodRatings = await _db.FoodRatings
                    .Select(fr => new { fr.UserId, fr.FoodId })
                    .ToListAsync();

                foreach (var food in foods)
                {
                    var votingUsers = allUsersForRating
                        .OrderBy(x => random.Next())
                        .Take(random.Next(3, 6))
                        .ToList();

                    foreach (var user in votingUsers)
                    {
                        bool alreadyExists = existingFoodRatings.Any(fr => fr.UserId == user.Id && fr.FoodId == food.Id)
                            || foodRatings.Any(fr => fr.UserId == user.Id && fr.FoodId == food.Id);

                        if (alreadyExists)
                            continue;

                        foodRatings.Add(new FoodRating
                        {
                            FoodId = food.Id,
                            UserId = user.Id,
                            Score = random.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
                        });
                    }
                }

                _db.FoodRatings.AddRange(foodRatings);
                await _db.SaveChangesAsync();

                // RestaurantRatings
                var allRestaurants = await _db.Restaurants.ToListAsync();
                var allUsers = await _db.Users.ToListAsync();

                var existingRatings = await _db.RestaurantRatings
                    .Select(r => new { r.UserId, r.RestaurantId })
                    .ToListAsync();

                var ratings = new List<RestaurantRating>();

                foreach (var restaurant in allRestaurants)
                {
                    var votingUsers = allUsers
                        .Where(u => u.PhoneNumber != null && u.PhoneNumber.StartsWith("+98"))
                        .OrderBy(x => random.Next())
                        .Take(random.Next(3, 6))
                        .ToList();

                    foreach (var user in votingUsers)
                    {
                        bool alreadyExists = existingRatings.Any(er => er.UserId == user.Id && er.RestaurantId == restaurant.Id)
                            || ratings.Any(r => r.UserId == user.Id && r.RestaurantId == restaurant.Id);

                        if (alreadyExists)
                            continue;

                        ratings.Add(new RestaurantRating
                        {
                            RestaurantId = restaurant.Id,
                            UserId = user.Id,
                            Score = random.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
                        });
                    }
                }

                _db.RestaurantRatings.AddRange(ratings);

                // 8️⃣ Ad Banner (one active ad for now)
                if (!await _db.RestaurantAdBanners.AnyAsync())
                {
                    var randomRestaurant = await _db.Restaurants.FirstOrDefaultAsync();
                    if (randomRestaurant != null)
                    {
                        var adBanner = new RestaurantAdBanner
                        {
                            RestaurantId = randomRestaurant.Id,
                            ImageUrl = "/img/optcropban.jpg",
                            StartDate = DateTime.UtcNow.AddDays(-2),
                            EndDate = DateTime.UtcNow.AddDays(5)
                        };

                        _db.RestaurantAdBanners.Add(adBanner);
                    }
                }

                await _db.SaveChangesAsync();

                // 9️⃣ Customer account
                if (!await _db.Users.AnyAsync(u => u.PhoneNumber == "+989121112233"))
                {
                    var customer = new User
                    {
                        UserName = "+989121112233",
                        PhoneNumber = "+989121112233",
                        FullName = "مشتری نمونه"
                    };
                    await _userManager.CreateAsync(customer, "Customer123!");
                    await _userManager.AddToRoleAsync(customer, SD.Role_Customer);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
