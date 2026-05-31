using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class AdminSeeder : IDataSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly MenroDbContext _db;

    public AdminSeeder(
        UserManager<User> userManager,
        MenroDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public int Order => SeedOrder.Admin;

    public async Task SeedAsync()
    {
        const string adminEmail = "MenroAdmin@gmail.com";

        // 1. UPSERT USER
        var admin = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == adminEmail);

        if (admin == null)
        {
            admin = new User
            {
                UserName = "MenroAdmin_1",
                Email = adminEmail,
                FullName = "مدیر",
                PhoneNumber = "+989486813486",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, "@Admin123456");

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        // 2. SYNC ROLES (idempotent)
        var roles = await _userManager.GetRolesAsync(admin);

        if (!roles.Contains(SD.Role_Admin))
            await _userManager.AddToRoleAsync(admin, SD.Role_Admin);

        if (!roles.Contains(SD.Role_Owner))
            await _userManager.AddToRoleAsync(admin, SD.Role_Owner);

        // 3. UPSERT RESTAURANT
        var restaurant = await _db.Restaurants
            .FirstOrDefaultAsync(x => x.OwnerUserId == admin.Id);

        if (restaurant == null)
        {
            restaurant = new Restaurant
            {
                Name = "رستوران مدیریت سیستم",
                Slug = "admin-restaurant",
                Address = "تهران، دفتر مرکزی",
                ContactNumber = admin.PhoneNumber!,
                OpenTime = new TimeSpan(8, 0, 0),
                CloseTime = new TimeSpan(22, 0, 0),
                Description = "رستوران اختصاصی ادمین سیستم",

                NationalCode = "9999999999",
                BankAccountNumber = "0000000000",
                ShebaNumber = "IR000000000000000000000",

                OwnerUserId = admin.Id,
                RestaurantCategoryId = 1,

                CarouselImageUrl = "/img/res-slider.jpg",
                BannerImageUrl = "/img/res-card-1.png",
                ShopBannerImageUrl = "/img/ad-banner-1.jpg",
                LogoImageUrl = "/img/logo-orange.png",

                TableCount = 10,
                Status = Domain.Enums.RestaurantStatus.Approved,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Restaurants.AddAsync(restaurant);
        }
        else
        {
            // optional sync updates (safe fields)
            restaurant.Name = "رستوران مدیریت سیستم";
            restaurant.Address = "تهران، دفتر مرکزی";
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Admin synced.");
    }
}