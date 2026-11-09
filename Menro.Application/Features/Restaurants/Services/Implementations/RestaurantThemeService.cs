using Menro.Application.Common.Models;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantThemeService : IRestaurantThemeService
    {
        // ⚠️ باید دقیقاً با idهای restaurantThemes.js یکی باشد
        private static readonly HashSet<string> AllowedThemeIds = new()
        {
            "modern", "seafood", "fastfood", "beach", "traditional",
            "italian", "asian", "cafe", "garden", "dessert"
        };

        private readonly IUnitOfWork _uow;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IRestaurantBannerService _restaurantBannerService;

        public RestaurantThemeService(
            IUnitOfWork uow,
            IRestaurantRepository restaurantRepository,
            IRestaurantBannerService restaurantBannerService)
        {
            _uow = uow;
            _restaurantRepository = restaurantRepository;
            _restaurantBannerService = restaurantBannerService;
        }

        public async Task<string?> GetThemeIdAsync(string userId)
        {
            var restaurant = await _uow.Restaurant.GetByOwnerUserIdAsync(userId);
            return restaurant?.ThemeId;
        }

        public async Task<Result> SaveThemeAsync(string userId, string themeId)
        {
            if (!AllowedThemeIds.Contains(themeId))
                return Result.Failure("قالب انتخاب‌شده معتبر نیست.", ErrorCode.Invalid);

            var restaurant = await _uow.Restaurant.GetByOwnerUserIdAsync(userId);
            if (restaurant == null)
                return Result.Failure("رستوران یافت نشد.", ErrorCode.NotFound);

            restaurant.ThemeId = themeId;
            await _uow.SaveChangesAsync();

            // بنر کش می‌شود (مثل RestaurantRatingService)؛ بدون این، تم جدید تا انقضای کش دیده نمی‌شود
            _restaurantRepository.InvalidateRestaurantBanner(restaurant.Slug);
            _restaurantBannerService.InvalidateCache(restaurant.Slug);

            return Result.Success();
        }
    }
}