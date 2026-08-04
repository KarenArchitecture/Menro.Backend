using Menro.Application.Common.Interfaces;
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
    private readonly IMediaStorageProvider _mediaStorage;
    private readonly MediaStorageOptions _mediaOptions;

    private readonly Random _rand = new(42);

    private const int TargetAdBanners = 5;

    public DemoRestaurantAdSeeder(
        MenroDbContext db,
        IMediaStorageProvider mediaStorage,
        IOptions<MediaStorageOptions> mediaOptions)
    {
        _db = db;
        _mediaStorage = mediaStorage;
        _mediaOptions = mediaOptions.Value;
    }
    public int Order => SeedOrder.RestaurantAd;
    public async Task SeedAsync()
    {
        if (await _db.RestaurantAds.AnyAsync())
        {
            Console.WriteLine(
                "[Seed] Demo restaurant ads already seeded.");

            return;
        }

        var now = DateTime.UtcNow;

        var restaurants = await _db.Restaurants
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.Status == RestaurantStatus.Approved)
            .OrderBy(x => x.Id)
            .Take(10)
            .ToListAsync();

        if (!restaurants.Any())
        {
            Console.WriteLine(
                "[Seed] No restaurants found for ad seeding.");

            return;
        }

        // 🔧 Real sample bytes, saved per-entity through the actual media
        // pipeline below — same reasoning as DemoRestaurantSeeder. Registry
        // documents both RestaurantAdBanner and RestaurantAdCarousel as
        // entity-scoped by restaurantId, so we use restaurant.Id (known
        // up-front here, no need to save first).
        var carouselBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/ads/carousel/res-slider.jpg"));
        var bannerBytes = File.ReadAllBytes(Path.Combine(_mediaOptions.RootPath, "media/img/ads/banner/ad-banner-1.jpg"));

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

        var carouselCount =
            Math.Min(5, restaurants.Count);

        for (int i = 0; i < carouselCount; i++)
        {
            var restaurant = restaurants[i];

            var billingType =
                i % 2 == 0
                    ? AdBillingType.PerDay
                    : AdBillingType.PerClick;

            var purchasedUnits =
                billingType == AdBillingType.PerDay
                    ? _rand.Next(7, 15)
                    : _rand.Next(500, 1500);

            var carouselImgResult = await _mediaStorage.SaveBytesAsync(
                MediaCategory.RestaurantAdCarousel, carouselBytes, ".jpg", restaurant.Id.ToString());

            var ad = new RestaurantAd
            {
                RestaurantId = restaurant.Id,

                PlacementType =
                    AdPlacementType.MainSlider,

                BillingType = billingType,

                ImageFileName = carouselImgResult.FileName,

                TargetUrl = restaurant.Slug,

                CommercialText =
                    carouselTexts[
                        i % carouselTexts.Length],

                CreatedAt =
                    now.AddDays(-_rand.Next(1, 5)),

                StartDate = now.AddDays(-1),

                EndDate =
                    billingType == AdBillingType.PerDay
                        ? now.AddDays(purchasedUnits)
                        : now.AddMonths(3),

                PurchasedUnits = purchasedUnits,

                ConsumedUnits = 0,

                Cost =
                    billingType == AdBillingType.PerDay
                        ? purchasedUnits * 250_000
                        : purchasedUnits * 2_000,

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

            var billingType =
                i % 2 == 0
                    ? AdBillingType.PerView
                    : AdBillingType.PerClick;

            var purchasedUnits =
                billingType == AdBillingType.PerView
                    ? _rand.Next(3000, 8000)
                    : _rand.Next(400, 1200);

            var bannerImgResult = await _mediaStorage.SaveBytesAsync(
                MediaCategory.RestaurantAdBanner, bannerBytes, ".jpg", restaurant.Id.ToString());

            var ad = new RestaurantAd
            {
                RestaurantId = restaurant.Id,

                PlacementType =
                    AdPlacementType.FullscreenBanner,

                BillingType = billingType,

                ImageFileName = bannerImgResult.FileName,

                TargetUrl = restaurant.Slug,

                CommercialText =
                    bannerTexts[
                        i % bannerTexts.Length],

                CreatedAt =
                    now.AddDays(-_rand.Next(1, 5)),

                StartDate = now.AddDays(-1),

                EndDate = now.AddMonths(3),

                PurchasedUnits = purchasedUnits,

                ConsumedUnits = 0,

                Cost =
                    billingType == AdBillingType.PerView
                        ? purchasedUnits * 500
                        : purchasedUnits * 2500,

                Status = AdStatus.Approved,

                AdminNotes = null
            };

            _db.RestaurantAds.Add(ad);
        }

        await _db.SaveChangesAsync();

        Console.WriteLine(
            "[Seed] Demo restaurant ads seeded.");
    }
}