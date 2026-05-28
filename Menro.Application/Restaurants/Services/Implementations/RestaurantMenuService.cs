using Menro.Application.Common.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Restaurants.Services.Implementations
{
    public class RestaurantMenuService : IRestaurantMenuService
    {
        private readonly IFoodRepository _foodRepo;
        private readonly IFileUrlService _fileUrlService;

        public RestaurantMenuService(
            IFoodRepository foodRepo,
            IFileUrlService fileUrlService)
        {
            _foodRepo = foodRepo;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<RestaurantMenuDto>> GetMenuBySlugAsync(string slug)
        {
            // 1. گرفتن دیتا از Repository (خروجی Entity است)
            var foods = await _foodRepo.GetRestaurantMenuBySlugAsync(slug);

            if (foods == null || !foods.Any())
                return new List<RestaurantMenuDto>();

            // 2. گروه‌بندی و تبدیل به DTO در لایه Application
            var grouped = foods
                .GroupBy(f => f.CustomFoodCategoryId ?? f.GlobalFoodCategoryId)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var first = g.First();

                    // تعیین عنوان دسته بندی
                    var categoryTitle = first.CustomFoodCategory?.Name
                                        ?? first.GlobalFoodCategory?.Name
                                        ?? "نامشخص";

                    // تعیین آیکون دسته بندی
                    var iconFile = first.CustomFoodCategory?.Icon?.FileName
                                   ?? first.GlobalFoodCategory?.Icon?.FileName;

                    var svgIconUrl = string.IsNullOrWhiteSpace(iconFile)
                        ? string.Empty
                        : _fileUrlService.BuildIconUrl(iconFile);

                    return new RestaurantMenuDto
                    {
                        CategoryId = g.Key ?? 0,
                        CategoryKey = categoryTitle.Replace(" ", "-"),
                        CategoryTitle = categoryTitle,
                        SvgIcon = svgIconUrl,

                        Foods = g.Select(f =>
                        {
                            // منطق تعیین قیمت (بدون تغییر در منطق تیم شما)
                            var displayPrice = f.Price;
                            if (f.Variants != null && f.Variants.Any())
                            {
                                var defaultVariant = f.Variants.FirstOrDefault(v => v.IsDefault == true)
                                                   ?? f.Variants.FirstOrDefault(v => v.IsAvailable)
                                                   ?? f.Variants.First();
                                displayPrice = defaultVariant.Price;
                            }

                            return new FoodCardDto
                            {
                                Id = f.Id,
                                Name = f.Name,
                                Ingredients = f.Ingredients,
                                Price = displayPrice,
                                ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                                    ? null
                                    : _fileUrlService.BuildFoodImageUrl(f.ImageUrl),

                                // استفاده از فیلدهای NotMapped که حالا Ratings لود شده‌اند
                                Rating = f.AverageRating,
                                Voters = f.VotersCount,

                                RestaurantName = f.Restaurant?.Name ?? string.Empty,
                                RestaurantCategory = categoryTitle
                            };
                        }).ToList()
                    };
                })
                .ToList();

            return grouped;
        }

    }
}
