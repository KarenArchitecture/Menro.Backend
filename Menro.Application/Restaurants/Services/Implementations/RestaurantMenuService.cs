using Menro.Application.Foods.DTOs;
using Menro.Application.Restaurants.DTOs;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Restaurants.Services.Implementations
{
    /// <summary>
    /// Builds the public-facing menu structure for a restaurant:
    /// categories → foods (FoodCardDto).
    /// </summary>
    public class RestaurantMenuService : IRestaurantMenuService
    {
        private readonly IFoodRepository _foodRepo;

        public RestaurantMenuService(IFoodRepository foodRepo)
        {
            _foodRepo = foodRepo;
        }

        public async Task<List<RestaurantMenuDto>> GetMenuBySlugAsync(string slug)
        {
            var foods = await _foodRepo.GetRestaurantMenuBySlugAsync(slug);

            if (foods == null || foods.Count == 0)
                return new List<RestaurantMenuDto>();

            // group by category (custom OR global)
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

                    var svgIconUrl = iconFile != null ? $"/icons/{iconFile}" : "";

                    return new RestaurantMenuDto
                    {
                        CategoryId = g.Key ?? 0,
                        CategoryKey = categoryTitle.Replace(" ", "-"),
                        CategoryTitle = categoryTitle,
                        SvgIcon = svgIconUrl,
                        Foods = g.Select(f =>
                        {
                            var displayPrice = f.Price;

                            // ✅ if variants exist, use default variant price
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
                                Price = displayPrice, // ✅ changed (was f.Price)
                                ImageUrl = f.ImageUrl,
                                Rating = f.AverageRating,
                                Voters = f.VotersCount,
                                RestaurantName = f.Restaurant.Name,
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
