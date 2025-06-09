using Menro.Application.SD;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Domain.Interfaces;

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
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() != 0)
                {
                    _db.Database.Migrate();
                }
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Owner)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();

                    // Admin User
                    _userManager.CreateAsync(new User()
                    {
                        UserName = "MenroAdmin_1",
                        Email = "MenroAdmin@gmail.com",
                        FullName = "Admin",
                        NormalizedEmail = "MenroAdmin@gmail.com",
                        NormalizedUserName = "MenroAdmin@gmail.com".ToUpper(),
                        PhoneNumber = "+989486813486",
                    },
                    password: "@Admin123456"
                    ).GetAwaiter().GetResult();
                    User? adminUser = _db.Users.FirstOrDefault(u => u.Email == "MenroAdmin@gmail.com");
                    _userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();

                    // Owner User
                    _userManager.CreateAsync(new User()
                    {
                        UserName = "CafeOwner1",
                        Email = "owner1@menro.com",
                        FullName = "صاحب رستوران نمونه",
                        NormalizedEmail = "OWNER1@MENRO.COM",
                        NormalizedUserName = "OWNER1@MENRO.COM",
                        PhoneNumber = "+989123456789",
                    },
                    password: "Owner123!").GetAwaiter().GetResult();

                    var ownerUser = _db.Users.FirstOrDefault(u => u.Email == "owner1@menro.com");
                    _userManager.AddToRoleAsync(ownerUser, SD.Role_Owner).GetAwaiter().GetResult();

                    // افزودن رستوران وابسته به Owner User
                    var restaurant = new Restaurant()
                    {
                        // Id را ننویسید تا EF خودش مقدار مناسب بگذارد
                        Name = "کافه منرو",
                        Address = "خیابان ولیعصر، پلاک ۱۲۳",
                        NationalCode = "0012345678",
                        BankAccountNumber = "1234567890",
                        ShebaNumber = "IR820540102680020817909002",
                        RestaurantCategoryId = 5, // از داده‌های HasData خوانده می‌شود
                        OwnerUserId = ownerUser.Id
                    };
                    _db.Restaurants.Add(restaurant);
                    _db.SaveChanges(); // ذخیره برای گرفتن Id

                    // اضافه کردن دسته بندی‌های غذایی وابسته به رستوران ایجاد شده
                    var foodCategories = new List<FoodCategory>()
                        {
                            new FoodCategory { Name = "نوشیدنی سرد", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "نوشیدنی گرم", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "پیتزا", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "پاستا", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "سالاد", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "دسر", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "سوپ", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "برگر", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "غذای دریایی", RestaurantId = restaurant.Id },
                            new FoodCategory { Name = "پیش‌غذا", RestaurantId = restaurant.Id },
                        };
                    _db.FoodCategories.AddRange(foodCategories);
                    _db.SaveChanges();
                }

            }

            catch (Exception e)
            {
                throw;
            }
        }

    }
}
