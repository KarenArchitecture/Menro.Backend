using Menro.Application.Common.Media;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoRestaurantAdSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly MediaStorageOptions _mediaOptions;
    private readonly Random _rand = new(42);
    private const int TargetAdBanners = 5;

    // These are the SOURCE files, expected to already exist flat under
    // wwwroot/media/img/ads/banner/ and wwwroot/media/img/ads/carousel/
    // (per the current wwwroot layout). The seeder copies each one into
    // the per-restaurant "{restaurantId}/original/" folder that
    // LocalDiskMediaStorageProvider.GetUrl() actually expects, since
    // both RestaurantAdBanner and RestaurantAdCarousel are entity-scoped,
    // image-processed categories.
    private static readonly string[] CarouselAdImages =
    {
        "res-slider.jpg",
        "optcropban.jpg",
        "top-banner.png"
    };

    private static readonly string[] FullscreenAdImages =
    {
        "ad-banner-1.jpg",
        "ad-banner-2.png",
        "top-banner.png"
    };

    public DemoRestaurantAdSeeder(MenroDbContext db, IOptions<MediaStorageOptions> mediaOptions)
    {
        _db = db;
        _mediaOptions = mediaOptions.Value;
    }

    public int Order => SeedOrder.RestaurantAd;

    public async Task SeedAsync()
    {
        if (await _db.RestaurantAds.AnyAsync())
        {
            Console.WriteLine("[Seed] Demo restaurant ads already seeded.");
            return;
        }

        var now = DateTime.UtcNow;

        var restaurants = await _db.Restaurants
            .Where(x => x.IsActive && !x.IsDeleted && x.Status == RestaurantStatus.Approved)
            .OrderBy(x => x.Id)
            .Take(10)
            .ToListAsync();

        if (!restaurants.Any())
        {
            Console.WriteLine("[Seed] No restaurants found for ad seeding.");
            return;
        }

        var carouselTexts = new[]
        {
            "افتتاحیه ویژه؛ تجربه‌ای تازه از طعم",
            "پیشنهاد ویژه امروز فقط در منرو",
            "غذاهای محبوب با ارسال سریع",
            "طعم متفاوت، انتخاب هوشمندانه",
            "رستوران منتخب این هفته"
        };

        var bannerTexts = new[]
        {
            "تخفیف ویژه سفارش آنلاین",
            "پیشنهاد امروز را از دست ندهید",
            "ارسال سریع، غذای تازه",
            "طعم محبوب کاربران منرو",
            "ویژه مشتریان منرو"
        };

        /* ============================================================
           Main Slider Ads
        ============================================================ */
        var carouselCount = Math.Min(5, restaurants.Count);
        for (int i = 0; i < carouselCount; i++)
        {
            var restaurant = restaurants[i];
            var billingType = i % 2 == 0 ? AdBillingType.PerDay : AdBillingType.PerClick;
            var purchasedUnits = billingType == AdBillingType.PerDay ? _rand.Next(7, 15) : _rand.Next(500, 1500);
            var fileName = CarouselAdImages[i % CarouselAdImages.Length];

            CopySeedImageIntoEntityFolder(
                MediaCategory.RestaurantAdCarousel,
                sourceFolderRelative: "media/img/ads/carousel",
                fileName,
                entityId: restaurant.Id.ToString());

            var ad = new RestaurantAd
            {
                RestaurantId = restaurant.Id,
                PlacementType = AdPlacementType.MainSlider,
                BillingType = billingType,
                ImageFileName = fileName,
                TargetUrl = restaurant.Slug,
                CommercialText = carouselTexts[i % carouselTexts.Length],
                CreatedAt = now.AddDays(-_rand.Next(1, 5)),
                StartDate = now.AddDays(-1),
                EndDate = billingType == AdBillingType.PerDay ? now.AddDays(purchasedUnits) : now.AddMonths(3),
                PurchasedUnits = purchasedUnits,
                ConsumedUnits = 0,
                Cost = billingType == AdBillingType.PerDay ? purchasedUnits * 250_000 : purchasedUnits * 2_000,
                Status = AdStatus.Approved,
                AdminNotes = null
            };
            _db.RestaurantAds.Add(ad);
        }

        /* ============================================================
           Fullscreen Banner Ads
        ============================================================ */
        var bannerRestaurants = restaurants
            .OrderBy(_ => Guid.NewGuid())
            .Take(TargetAdBanners)
            .ToList();

        for (int i = 0; i < bannerRestaurants.Count; i++)
        {
            var restaurant = bannerRestaurants[i];
            var billingType = i % 2 == 0 ? AdBillingType.PerView : AdBillingType.PerClick;
            var purchasedUnits = billingType == AdBillingType.PerView ? _rand.Next(3000, 8000) : _rand.Next(400, 1200);
            var fileName = FullscreenAdImages[i % FullscreenAdImages.Length];

            CopySeedImageIntoEntityFolder(
                MediaCategory.RestaurantAdBanner,
                sourceFolderRelative: "media/img/ads/banner",
                fileName,
                entityId: restaurant.Id.ToString());

            var ad = new RestaurantAd
            {
                RestaurantId = restaurant.Id,
                PlacementType = AdPlacementType.FullscreenBanner,
                BillingType = billingType,
                ImageFileName = fileName,
                TargetUrl = restaurant.Slug,
                CommercialText = bannerTexts[i % bannerTexts.Length],
                CreatedAt = now.AddDays(-_rand.Next(1, 5)),
                StartDate = now.AddDays(-1),
                EndDate = now.AddMonths(3),
                PurchasedUnits = purchasedUnits,
                ConsumedUnits = 0,
                Cost = billingType == AdBillingType.PerView ? purchasedUnits * 500 : purchasedUnits * 2500,
                Status = AdStatus.Approved,
                AdminNotes = null
            };
            _db.RestaurantAds.Add(ad);
        }

        await _db.SaveChangesAsync();
        Console.WriteLine("[Seed] Demo restaurant ads seeded (with per-restaurant image copies).");
    }

    // Copies a flat "source" image (e.g. wwwroot/media/img/ads/banner/ad-banner-1.jpg)
    // into the exact per-restaurant "original" folder that GetUrl() will compute
    // for that MediaCategory, so the URLs the frontend receives actually resolve.
    private void CopySeedImageIntoEntityFolder(
        MediaCategory category,
        string sourceFolderRelative,
        string fileName,
        string entityId)
    {
        var sourcePath = Path.Combine(_mediaOptions.RootPath, sourceFolderRelative, fileName);
        if (!File.Exists(sourcePath))
        {
            Console.WriteLine($"[Seed] WARNING: source ad image not found, skipping copy: {sourcePath}");
            return;
        }

        var destFolderRelative = category switch
        {
            MediaCategory.RestaurantAdBanner => $"media/img/ads/banner/{entityId}/original",
            MediaCategory.RestaurantAdCarousel => $"media/img/ads/carousel/{entityId}/original",
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };

        var destDir = Path.Combine(_mediaOptions.RootPath, destFolderRelative);
        Directory.CreateDirectory(destDir);

        var destPath = Path.Combine(destDir, fileName);
        if (!File.Exists(destPath))
        {
            File.Copy(sourcePath, destPath, overwrite: false);
        }
    }
}