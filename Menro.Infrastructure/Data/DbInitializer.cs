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

        private string GenerateSlug(int index)
        {
            return $"restaurant-number-{index}";
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                    await _db.Database.MigrateAsync();

                // 1️⃣ Roles
                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                // 2️⃣ Admin user
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

                // Sample SVG icon string
                var sampleSvg = """
                <svg width="6" height="9" viewBox="0 0 6 9" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M5.07174 0.0373714C5.15805 0.00893344 5.24959 -0.00348178 5.34111 0.000837744C5.52489 0.0107347 5.69693 0.0874758 5.81958 0.214267C5.94329 0.340265 6.00789 0.506412 5.99923 0.676314C5.99057 0.846216 5.90935 1.00602 5.77336 1.12072L1.73486 4.50298L5.77336 7.88146C5.90935 7.99616 5.99057 8.15597 5.99923 8.32587C6.00789 8.49577 5.94329 8.66192 5.81958 8.78792C5.69614 8.91422 5.52352 8.99017 5.33955 8.99911C5.15558 9.00805 4.97528 8.94927 4.83816 8.83563L0.228749 4.97755C0.156718 4.91728 0.0991497 4.84375 0.0597519 4.76169C0.0203541 4.67963 0 4.59086 0 4.50109C0 4.41133 0.0203541 4.32256 0.0597519 4.2405C0.0991497 4.15843 0.156718 4.08491 0.228749 4.02464L4.83816 0.166554C4.90606 0.109711 4.98543 0.0658094 5.07174 0.0373714Z" fill="white"/></svg>
                """;

                // 3️⃣ Owners and Restaurants
                for (int i = 1; i <= 15; i++)
                {
                    string email = $"owner{i}@menro.com";
                    if (await _db.Users.AnyAsync(u => u.Email == email))
                        continue;

                    var owner = new User
                    {
                        UserName = $"+98912{345678 + i}",
                        Email = email,
                        FullName = $"رستوران‌دار {i}",
                        PhoneNumber = $"+98912{345678 + i}"
                    };
                    await _userManager.CreateAsync(owner, "Owner123!");
                    await _userManager.AddToRoleAsync(owner, SD.Role_Owner);

                    var restaurant = new Restaurant
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
                        CarouselImageUrl = "/img/res-slider.png",
                        BannerImageUrl = "/img/res-cards.png",
                        IsFeatured = (i % 3 == 0),
                        IsActive = true,
                        IsApproved = true,
                        Slug = GenerateSlug(i)
                    };

                    _db.Restaurants.Add(restaurant);
                    await _db.SaveChangesAsync();

                    // Add 5 categories per restaurant with SVG, avoid duplicates
                    var existingCategoriesCount = await _db.FoodCategories.CountAsync(fc => fc.RestaurantId == restaurant.Id);
                    if (existingCategoriesCount == 0)
                    {
                        var categories = Enumerable.Range(1, 5).Select(j => new FoodCategory
                        {
                            Name = $"دسته {j}",
                            RestaurantId = restaurant.Id,
                            SvgIcon = sampleSvg
                        }).ToList();

                        _db.FoodCategories.AddRange(categories);
                        await _db.SaveChangesAsync();
                    }
                }

                // 4️⃣ Foods
                var allFoodCategories = await _db.FoodCategories.Include(fc => fc.Restaurant).ToListAsync();
                var allUsers = await _db.Users.ToListAsync();
                var rand = new Random();

                foreach (var category in allFoodCategories)
                {
                    // Check if foods exist for this category
                    bool foodsExist = await _db.Foods.AnyAsync(f => f.FoodCategoryId == category.Id);
                    if (foodsExist)
                        continue;

                    var foods = new List<Food>();
                    for (int i = 1; i <= 5; i++)
                    {
                        foods.Add(new Food
                        {
                            Name = $"غذای نمونه {i} دسته {category.Name}",
                            Ingredients = "مواد اولیه نمونه",
                            Price = rand.Next(15000, 80000),
                            FoodCategoryId = category.Id,
                            RestaurantId = category.RestaurantId,
                            ImageUrl = "/img/drink.png",
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 30))
                        });
                    }

                    _db.Foods.AddRange(foods);
                    await _db.SaveChangesAsync();
                }

                // 5️⃣ Food Ratings
                var allFoods = await _db.Foods.ToListAsync();
                foreach (var food in allFoods)
                {
                    // Add ratings only if none exist for this food
                    bool foodHasRatings = await _db.FoodRatings.AnyAsync(fr => fr.FoodId == food.Id);
                    if (foodHasRatings)
                        continue;

                    var foodRatings = new List<FoodRating>();
                    var raters = allUsers.OrderBy(_ => rand.Next()).Take(rand.Next(3, 6));
                    foreach (var user in raters)
                    {
                        foodRatings.Add(new FoodRating
                        {
                            FoodId = food.Id,
                            UserId = user.Id,
                            Score = rand.Next(3, 6),
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 10))
                        });
                    }
                    _db.FoodRatings.AddRange(foodRatings);
                    await _db.SaveChangesAsync();
                }

                // 6️⃣ Restaurant Ratings
                var allRestaurants = await _db.Restaurants.ToListAsync();
                foreach (var res in allRestaurants)
                {
                    // Check if ratings exist for this restaurant
                    bool hasRatings = await _db.RestaurantRatings.AnyAsync(rr => rr.RestaurantId == res.Id);
                    if (hasRatings)
                        continue;

                    var restaurantRatings = new List<RestaurantRating>();
                    var raters = allUsers.OrderBy(_ => rand.Next()).Take(rand.Next(3, 6));
                    foreach (var user in raters)
                    {
                        // Check if rating for user-restaurant already exists to avoid duplicates
                        bool exists = await _db.RestaurantRatings.AnyAsync(rr => rr.RestaurantId == res.Id && rr.UserId == user.Id);
                        if (!exists)
                        {
                            restaurantRatings.Add(new RestaurantRating
                            {
                                RestaurantId = res.Id,
                                UserId = user.Id,
                                Score = rand.Next(3, 6),
                                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 10))
                            });
                        }
                    }
                    if (restaurantRatings.Count > 0)
                    {
                        _db.RestaurantRatings.AddRange(restaurantRatings);
                        await _db.SaveChangesAsync();
                    }
                }

                // 7️⃣ Banner
                if (!await _db.RestaurantAdBanners.AnyAsync())
                {
                    var firstRes = await _db.Restaurants.FirstOrDefaultAsync();
                    if (firstRes != null)
                    {
                        _db.RestaurantAdBanners.Add(new RestaurantAdBanner
                        {
                            RestaurantId = firstRes.Id,
                            ImageUrl = "/img/optcropban.jpg",
                            StartDate = DateTime.UtcNow.AddDays(-2),
                            EndDate = DateTime.UtcNow.AddDays(5)
                        });
                        await _db.SaveChangesAsync();
                    }
                }

                // 8️⃣ Customer User
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
            catch (Exception ex)
            {
                throw new Exception("Database seeding failed: " + ex.Message, ex);
            }
        }
    }
}
