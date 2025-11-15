using Menro.Application.Common.Interfaces;
using Menro.Application.FoodCategories.DTOs;
using Menro.Application.FoodCategories.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.FoodCategories.Services.Implementations
{
    /// <summary>
    /// Service for providing the food category filter section (Shop Page - Section 2).
    /// Retrieves and maps CustomFoodCategory + GlobalFoodCategory data into DTOs.
    /// </summary>
    public class RestaurantPageFoodCategoryService : IRestaurantPageFoodCategoryService
    {
        private readonly ICustomFoodCategoryRepository _customCategoryRepo;
        private readonly IFileUrlService _fileUrlService;

        public RestaurantPageFoodCategoryService(
            ICustomFoodCategoryRepository customCategoryRepo,
            IFileUrlService fileUrlService)
        {
            _customCategoryRepo = customCategoryRepo;
            _fileUrlService = fileUrlService;
        }

        /// <summary>
        /// Retrieves and maps all visible categories (custom + global) for a given restaurant.
        /// </summary>
        public async Task<List<RestaurantFoodCategoryDto>> GetRestaurantCategoriesAsync(string restaurantSlug, CancellationToken ct = default)
        {
            var categories = await _customCategoryRepo.GetActiveByRestaurantSlug_WithIconsAsync(restaurantSlug, ct);

            var list = categories.Select(c =>
            {
                var iconFileName =
                    c.Icon?.FileName ??
                    c.GlobalCategory?.Icon?.FileName ?? string.Empty;

                var svgIcon = string.IsNullOrEmpty(iconFileName)
                    ? string.Empty
                    : _fileUrlService.BuildIconUrl(iconFileName);

                return new RestaurantFoodCategoryDto
                {
                    Id = c.Id.ToString(),
                    Name = !string.IsNullOrWhiteSpace(c.Name)
                        ? c.Name
                        : (c.GlobalCategory?.Name ?? "دسته‌بندی"),
                    SvgIcon = svgIcon,
                    IsGlobal = c.GlobalCategoryId.HasValue
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

            return list;
        }
    }
}
