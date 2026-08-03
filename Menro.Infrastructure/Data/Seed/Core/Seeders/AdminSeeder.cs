using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Domain.Entities.Music;
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
        const string ownerEmail = "owner@menro.com";

        // =========================
        // 1. ADMIN UPSERT
        // =========================
        var admin = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == adminEmail);

        if (admin == null)
        {
            admin = new User
            {
                UserName = "MenroAdmin_1",
                Email = adminEmail,
                FullName = "مدیر",
                PhoneNumber = "09486813486",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, "@Admin123456");

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        var adminRoles = await _userManager.GetRolesAsync(admin);

        if (!adminRoles.Contains(SD.Role_Admin))
            await _userManager.AddToRoleAsync(admin, SD.Role_Admin);

        if (!adminRoles.Contains(SD.Role_Owner))
            await _userManager.AddToRoleAsync(admin, SD.Role_Owner);


        // =========================
        // 2. OWNER (separate user)
        // =========================
        var owner = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == ownerEmail);

        if (owner == null)
        {
            owner = new User
            {
                UserName = "Owner_1",
                Email = ownerEmail,
                FullName = "صاحب رستوران نمونه",
                PhoneNumber = "09120000001",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(owner, "Owner123!");

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        var ownerRoles = await _userManager.GetRolesAsync(owner);

        // 🔥 فقط Owner role
        if (!ownerRoles.Contains(SD.Role_Owner))
            await _userManager.AddToRoleAsync(owner, SD.Role_Owner);

        // اگر اشتباهی Admin داشت حذفش کن
        if (ownerRoles.Contains(SD.Role_Admin))
            await _userManager.RemoveFromRoleAsync(owner, SD.Role_Admin);


        // =========================
        // 3. ADMIN RESTAURANT
        // =========================
        var adminRestaurant = await _db.Restaurants
            .FirstOrDefaultAsync(x => x.OwnerUserId == admin.Id);

        if (adminRestaurant == null)
        {
            adminRestaurant = new Restaurant
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

            await _db.Restaurants.AddAsync(adminRestaurant);
        }

        // =========================
        // 4. OWNER RESTAURANT
        // =========================
        var ownerRestaurant = await _db.Restaurants
            .FirstOrDefaultAsync(x => x.OwnerUserId == owner.Id);

        if (ownerRestaurant == null)
        {
            ownerRestaurant = new Restaurant
            {
                Name = "رستوران نمونه صاحب",
                Slug = "owner-restaurant",
                Address = "تهران، شعبه مرکزی",
                ContactNumber = owner.PhoneNumber!,

                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),

                Description = "رستوران متعلق به صاحب نمونه",

                NationalCode = "8888888888",
                BankAccountNumber = "1111111111",
                ShebaNumber = "IR111111111111111111111",

                OwnerUserId = owner.Id,
                RestaurantCategoryId = 2,

                CarouselImageUrl = "/img/res-slider.jpg",
                BannerImageUrl = "/img/res-card-1.png",
                ShopBannerImageUrl = "/img/ad-banner-1.jpg",
                LogoImageUrl = "/img/logo-orange.png",

                TableCount = 8,
                Status = Domain.Enums.RestaurantStatus.Approved,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Restaurants.AddAsync(ownerRestaurant);
        }

        await _db.SaveChangesAsync();

        // =========================
        // 5. DEFAULT PLAYLISTS
        // =========================

        var adminPlaylistExists = await _db.Playlists
            .AnyAsync(x => x.RestaurantId == adminRestaurant.Id);

        if (!adminPlaylistExists)
        {
            await _db.Playlists.AddAsync(new Playlist
            {
                Id = Guid.NewGuid(),
                RestaurantId = adminRestaurant.Id,
                Name = "پلی لیست اصلی",
                IsActive = true,
            });
        }

        var ownerPlaylistExists = await _db.Playlists
            .AnyAsync(x => x.RestaurantId == ownerRestaurant.Id);

        if (!ownerPlaylistExists)
        {
            await _db.Playlists.AddAsync(new Playlist
            {
                Id = Guid.NewGuid(),
                RestaurantId = ownerRestaurant.Id,
                Name = "پلی لیست اصلی",
                IsActive = true,
            });
        }

        await _db.SaveChangesAsync();

        Console.WriteLine("[Seed] Admin + Owner synced.");
    }
}