using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Foods.DTOs;
using Menro.Application.Features.Restaurants.DTOs;
using Menro.Application.Features.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Restaurants.Services.Implementations
{
    public class RestaurantMenuService : IRestaurantMenuService
    {
        private readonly IFoodRepository _foodRepo;
        private readonly IMediaStorageProvider _mediaStorage;

        public RestaurantMenuService(
            IFoodRepository foodRepo,
            IMediaStorageProvider mediaStorage)
        {
            _foodRepo = foodRepo;
            _mediaStorage = mediaStorage;
        }

        public async Task<List<RestaurantMenuDto>> GetMenuBySlugAsync(string slug)
        {
            var foods = await _foodRepo.GetRestaurantMenuBySlugAsync(slug);

            if (foods == null || !foods.Any())
                return new List<RestaurantMenuDto>();

            var grouped = foods
                .GroupBy(f => f.CustomFoodCategoryId ?? f.GlobalFoodCategoryId)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var first = g.First();

                    var categoryTitle = first.CustomFoodCategory?.Name
                                        ?? first.GlobalFoodCategory?.Name
                                        ?? "نامشخص";

                    var iconFile = first.CustomFoodCategory?.Icon?.FileName
                                   ?? first.GlobalFoodCategory?.Icon?.FileName;

                    var svgIconUrl = string.IsNullOrWhiteSpace(iconFile)
                        ? string.Empty
                        : _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, iconFile);

                    return new RestaurantMenuDto
                    {
                        CategoryId = g.Key ?? 0,
                        CategoryKey = categoryTitle.Replace(" ", "-"),
                        CategoryTitle = categoryTitle,
                        SvgIcon = svgIconUrl,

                        Foods = g.Select(f =>
                        {
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
                                    : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl),

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