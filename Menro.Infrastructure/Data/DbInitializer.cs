//using Menro.Application.SD;
//using Menro.Domain.Entities;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Menro.Domain.Interfaces;

//namespace Menro.Infrastructure.Data
//{
//    public class DbInitializer : IDbInitializer
//    {
//        private readonly MenroDbContext _db;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly UserManager<User> _userManager;


//        public DbInitializer(MenroDbContext db,
//            RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
//        {
//            _db = db;
//            _roleManager = roleManager;
//            _userManager = userManager;
//        }

//        public async Task InitializeAsync()
//        {
//            try
//            {
//                if (_db.Database.GetPendingMigrations().Any())
//                {
//                    _db.Database.Migrate();
//                }
//                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
//                {
//                    // Create roles using await
//                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
//                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
//                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));

//                    // Admin User
//                    await _userManager.CreateAsync(new User()
//                    {
//                        UserName = "MenroAdmin_1",
//                        Email = "MenroAdmin@gmail.com",
//                        FullName = "Admin",
//                        NormalizedEmail = "MenroAdmin@gmail.com",
//                        NormalizedUserName = "MenroAdmin@gmail.com".ToUpper(),
//                        PhoneNumber = "+989486813486",
//                    },
//                    password: "@Admin123456");

//                    // Find the user asynchronously
//                    User? adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == "MenroAdmin@gmail.com");
//                    if (adminUser != null)
//                    {
//                        await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);

//                        // Owner User
//                        await _userManager.CreateAsync(new User()
//                        {
//                            UserName = "CafeOwner1",
//                            Email = "owner1@menro.com",
//                            FullName = "صاحب رستوران نمونه",
//                            NormalizedEmail = "OWNER1@MENRO.COM",
//                            NormalizedUserName = "OWNER1@MENRO.COM",
//                            PhoneNumber = "+989123456789",
//                        },
//                        password: "Owner123!");

//                        User? ownerUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == "owner1@menro.com");
//                        if (ownerUser != null)
//                        {
//                            await _userManager.AddToRoleAsync(ownerUser, SD.Role_Owner);

//                            // Add the restaurant related to the Owner User
//                            var restaurant = new Restaurant()
//                            {
//                                // Id را ننویسید تا EF خودش مقدار مناسب بگذارد
//                                Name = "کافه منرو",
//                                Address = "خیابان ولیعصر، پلاک ۱۲۳",
//                                NationalCode = "0012345678",
//                                BankAccountNumber = "1234567890",
//                                ShebaNumber = "IR820540102680020817909002",
//                                RestaurantCategoryId = 5, // از داده‌های HasData خوانده می‌شود
//                                OwnerUserId = ownerUser.Id
//                            };
//                            _db.Restaurants.Add(restaurant);
//                            await _db.SaveChangesAsync(); // ذخیره برای گرفتن Id

//                            // اضافه کردن دسته بندی‌های غذایی وابسته به رستوران ایجاد شده
//                            var foodCategories = new List<FoodCategory>()
//                        {
//                            new FoodCategory { Name = "نوشیدنی سرد", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "نوشیدنی گرم", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "پیتزا", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "پاستا", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "سالاد", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "دسر", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "سوپ", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "برگر", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "غذای دریایی", RestaurantId = restaurant.Id },
//                            new FoodCategory { Name = "پیش‌غذا", RestaurantId = restaurant.Id },
//                        };
//                            _db.FoodCategories.AddRange(foodCategories);
//                            await _db.SaveChangesAsync();
//                        }

//                    }
//                } 
//            }

//            catch (Exception e)
//            {
//                throw;
//            }
//        }

//    }
//}

using Menro.Application.SD;
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

        public DbInitializer(MenroDbContext db,
            RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }

                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    // Create roles
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));

                    // Admin User
                    await _userManager.CreateAsync(new User()
                    {
                        UserName = "MenroAdmin_1",
                        Email = "MenroAdmin@gmail.com",
                        FullName = "Admin",
                        PhoneNumber = "+989486813486",
                    },
                    password: "@Admin123456");

                    User? adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == "MenroAdmin@gmail.com");
                    if (adminUser != null)
                    {
                        await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                    }

                    // Owner User 1
                    await _userManager.CreateAsync(new User()
                    {
                        UserName = "+989123456789",
                        Email = "owner1@menro.com",
                        FullName = "رضا احمدی",
                        PhoneNumber = "+989123456789",
                    },
                    password: "Owner123!");

                    User? ownerUser1 = await _db.Users.FirstOrDefaultAsync(u => u.Email == "owner1@menro.com");
                    if (ownerUser1 != null)
                    {
                        await _userManager.AddToRoleAsync(ownerUser1, SD.Role_Owner);

                        // Restaurant 1 for Owner 1
                        var restaurant1 = new Restaurant()
                        {
                            Name = "کافه منرو",
                            Address = "خیابان ولیعصر، پلاک ۱۲۳",
                            OpenTime = new TimeSpan(9, 0, 0),   // 9:00 AM
                            CloseTime = new TimeSpan(22, 0, 0), // 10:00 PM
                            Description = "بهترین منوساز ایرانی",
                            NationalCode = "0011111111",
                            BankAccountNumber = "1111111111",
                            RestaurantCategoryId = 5,
                            OwnerUserId = ownerUser1.Id,
                            CarouselImageUrl = "/img/res-slider.png",
                            IsFeatured = true
                        };
                        _db.Restaurants.Add(restaurant1);
                        await _db.SaveChangesAsync(); // Save to get restaurant1.Id

                        // *** FOOD CATEGORIES FOR RESTAURANT 1 ***
                        var cafeCategories1 = new List<FoodCategory>
                        {
                            new FoodCategory { Name = "نوشیدنی سرد", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "نوشیدنی گرم", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "پیتزا", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "پاستا", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "سالاد", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "دسر", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "سوپ", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "برگر", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "غذای دریایی", RestaurantId = restaurant1.Id },
                            new FoodCategory { Name = "پیش‌غذا", RestaurantId = restaurant1.Id },
                        };
                        _db.FoodCategories.AddRange(cafeCategories1);
                        await _db.SaveChangesAsync();

                        // ==========================================================
                        // --- START: Added Data (Keeping Your Original Structure) ---
                        // ==========================================================

                        // Restaurant 2 for Owner 1 (To test one user owning multiple restaurants)
                        var restaurant2 = new Restaurant()
                        {
                            Name = "کباب سرای اصیل",
                            Address = "میدان تجریش، بازار",
                            OpenTime = new TimeSpan(9, 0, 0),   // 9:00 AM
                            CloseTime = new TimeSpan(22, 0, 0), // 10:00 PM
                            Description = "بهترین کباب برگ و کوبیده در منطقه.",
                            NationalCode = "0022222222",
                            BankAccountNumber = "2222222222",
                            RestaurantCategoryId = 1,
                            OwnerUserId = ownerUser1.Id, // Same OwnerId
                            CarouselImageUrl = "/img/res-slider.png",
                            IsFeatured = true
                        };
                        _db.Restaurants.Add(restaurant2);
                        await _db.SaveChangesAsync();

                        // *** FOOD CATEGORIES FOR RESTAURANT 2 ***
                        var cafeCategories2 = new List<FoodCategory>
                        {
                            new FoodCategory { Name = "نوشیدنی سرد", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "نوشیدنی گرم", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "پیتزا", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "پاستا", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "سالاد", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "دسر", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "سوپ", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "برگر", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "غذای دریایی", RestaurantId = restaurant2.Id },
                            new FoodCategory { Name = "پیش‌غذا", RestaurantId = restaurant2.Id },
                        };
                        _db.FoodCategories.AddRange(cafeCategories2);
                        await _db.SaveChangesAsync();
                    }

                    // Save changes to get IDs before adding more dependent data
                    await _db.SaveChangesAsync();


                    // Owner User 2
                    await _userManager.CreateAsync(new User()
                    {
                        UserName = "+989129876543",
                        Email = "owner2@menro.com",
                        FullName = "سارا محمدی",
                        PhoneNumber = "+989129876543",
                    },
                    password: "Owner123!");

                    User? ownerUser2 = await _db.Users.FirstOrDefaultAsync(u => u.Email == "owner2@menro.com");
                    if (ownerUser2 != null)
                    {
                        await _userManager.AddToRoleAsync(ownerUser2, SD.Role_Owner);

                        // Restaurant 3 for Owner 2
                        var restaurant3 = new Restaurant()
                        {
                            Name = "پیتزا پینو",
                            Address = "سعادت آباد، بلوار دریا",
                            OpenTime = new TimeSpan(9, 0, 0),   // 9:00 AM
                            CloseTime = new TimeSpan(22, 0, 0), // 10:00 PM
                            Description = "پیتزای ایتالیایی اصیل با خمیر تازه.",
                            NationalCode = "0033333333",
                            BankAccountNumber = "3333333333",
                            RestaurantCategoryId = 6,
                            OwnerUserId = ownerUser2.Id,
                            CarouselImageUrl = "/img/res-slider.png",
                            IsFeatured = true
                        };
                        _db.Restaurants.Add(restaurant3);
                        await _db.SaveChangesAsync();

                        // *** FOOD CATEGORIES FOR RESTAURANT 3 ***
                        var cafeCategories3 = new List<FoodCategory>
                        {
                            new FoodCategory { Name = "نوشیدنی سرد", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "نوشیدنی گرم", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "پیتزا", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "پاستا", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "سالاد", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "دسر", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "سوپ", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "برگر", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "غذای دریایی", RestaurantId = restaurant3.Id },
                            new FoodCategory { Name = "پیش‌غذا", RestaurantId = restaurant3.Id },
                        };
                        _db.FoodCategories.AddRange(cafeCategories3);
                        await _db.SaveChangesAsync();
                    }

                    // Regular Customer User
                    await _userManager.CreateAsync(new User
                    {
                        UserName = "+989121112233",
                        PhoneNumber = "+989121112233",
                        FullName = "مشتری نمونه"
                    }, "Customer123!");

                    User? customerUser = await _db.Users.FirstOrDefaultAsync(u => u.UserName == "+989121112233");
                    if (customerUser != null)
                    {
                        await _userManager.AddToRoleAsync(customerUser, SD.Role_Customer);
                    }

                    // Final save for all new additions
                    await _db.SaveChangesAsync();

                    // ========================================================
                    // --- END: Added Data ---
                    // ========================================================
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}