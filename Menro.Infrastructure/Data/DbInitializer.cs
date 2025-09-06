using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data
{
    // seeds data whenever the project runs
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

        private async Task<string> GenerateUniqueSlugAsync(string baseName)
        {
            string baseSlug = Sluggify(baseName); // e.g. "Restaurant Number 1" -> "restaurant-number-1"
            string slug = baseSlug;
            int i = 1;

            while (await _db.Restaurants.AnyAsync(r => r.Slug == slug))
            {
                slug = $"{baseSlug}-{i}";
                i++;
            }
            return slug;
        }

        private string Sluggify(string text)
        {
            // simple example: replace spaces with -, remove invalid chars
            return text.Trim().ToLower().Replace(" ", "-");
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
                <svg width="43" height="43" viewBox="0 0 43 43" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M10.75 9.17527C10.0154 9.17527 9.40625 8.56611 9.40625 7.83152V4.69611C9.40625 3.96152 10.0154 3.35236 10.75 3.35236C11.4846 3.35236 12.0938 3.96152 12.0938 4.69611V7.83152C12.0938 8.58402 11.4846 9.17527 10.75 9.17527Z" fill="#999FA8"/>
                <path d="M17.9167 9.17527C17.1822 9.17527 16.573 8.56611 16.573 7.83152V4.69611C16.573 3.96152 17.1822 3.35236 17.9167 3.35236C18.6513 3.35236 19.2605 3.96152 19.2605 4.69611V7.83152C19.2605 8.58402 18.6513 9.17527 17.9167 9.17527Z" fill="#999FA8"/>
                <path d="M25.0833 9.17527C24.3487 9.17527 23.7395 8.56611 23.7395 7.83152V4.69611C23.7395 3.96152 24.3487 3.35236 25.0833 3.35236C25.8178 3.35236 26.427 3.96152 26.427 4.69611V7.83152C26.427 8.58402 25.8178 9.17527 25.0833 9.17527Z" fill="#999FA8"/>
                <path d="M39.8647 23.7426C39.8647 19.0485 36.2276 15.2502 31.6409 14.856C30.3151 12.6881 27.9501 11.2189 25.2267 11.2189H12.0222C7.8655 11.2189 4.47925 14.6052 4.47925 18.7618V19.7114H32.7697V18.7618C32.7697 18.4214 32.7159 18.081 32.6622 17.7585C35.2601 18.5289 37.1772 20.8939 37.1772 23.7426C37.1772 26.5555 35.3138 28.9026 32.7697 29.6909V21.503H4.47925V31.8768C4.47925 36.0335 7.8655 39.4197 12.0222 39.4197H25.2267C29.1684 39.4197 32.3755 36.3739 32.698 32.5039C36.783 31.6797 39.8647 28.0605 39.8647 23.7426Z" fill="#999FA8"/>
                </svg>
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

                    string restaurantName = $"Restaurant Number {i}";
                    string slug = await GenerateUniqueSlugAsync(restaurantName);

                    var restaurant = new Restaurant
                    {
                        Name = restaurantName,
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
                        Slug = slug,
                        CreatedAt = DateTime.Now.AddDays(-i)
                    };

                    _db.Restaurants.Add(restaurant);
                    await _db.SaveChangesAsync();

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
                    bool foodsExist = await _db.Foods.AnyAsync(f => f.FoodCategoryId == category.Id);
                    if (foodsExist)
                        continue;

                    var foods = new List<Food>();
                    for (int i = 1; i <= 5; i++)
                    {
                        foods.Add(new Food
                        {
                            Name = $"Sample Food {i} Category {category.Name}",
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
                var allFoods = await _db.Foods.Include(f => f.Restaurant).ToListAsync();
                foreach (var food in allFoods)
                {
                    bool hasRatings = await _db.FoodRatings.AnyAsync(r => r.FoodId == food.Id);
                    if (hasRatings) continue;

                    // pick 3 random users to rate this food
                    var randomUsers = allUsers.OrderBy(u => rand.Next()).Take(3).ToList();
                    foreach (var user in randomUsers)
                    {
                        var rating = new FoodRating
                        {
                            UserId = user.Id,
                            FoodId = food.Id,
                            Score = rand.Next(3, 6), // rating 3–5
                            CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 30))
                        };
                        _db.FoodRatings.Add(rating);
                    }
                }
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeding error: {ex.Message}");
                throw;
            }
        }
    }
}
