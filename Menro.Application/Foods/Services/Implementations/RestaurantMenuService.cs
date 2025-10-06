using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Application.Restaurants.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Implementations
{
    public class RestaurantMenuService : IRestaurantMenuService
    {
        private readonly IFoodRepository _foodRepository;

        public RestaurantMenuService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        //public async Task<List<RestaurantMenuDto>> GetRestaurantMenuBySlugAsync(string slug)
        //{
            //// 1️⃣  Pull all foods that belong to this restaurant
            //List<Food> foods = await _foodRepository.GetRestaurantMenuBySlugAsync(slug);

            //// 2️⃣  Group by category -> shape into DTOs
            //var sections = foods
            //.GroupBy(f => new                // ← value-based key
            //{
            //    f.FoodCategoryId,
            //    f.FoodCategory.Name,
            //    f.FoodCategory.SvgIcon
            //})
            //.Select(g => new RestaurantMenuDto
            //{
            //    CategoryId = g.Key.FoodCategoryId,
            //    CategoryKey = g.Key.FoodCategoryId.ToString(),
            //    CategoryTitle = g.Key.Name,
            //    SvgIcon = g.Key.SvgIcon,

            //    Foods = g.Select(f => new FoodCardDto
            //    {
            //        Id = f.Id,
            //        Name = f.Name,
            //        Ingredients = f.Ingredients,
            //        Price = f.Price,
            //        ImageUrl = f.ImageUrl,
            //        Rating = f.Ratings.Any() ? f.Ratings.Average(r => r.Score) : 0,
            //        Voters = f.Ratings.Count,
            //        RestaurantName = f.Restaurant?.Name ?? string.Empty,
            //        RestaurantCategory = f.Restaurant?.RestaurantCategory?.Name ?? string.Empty
            //    }).ToList()
            //})
            //.OrderBy(sec => sec.CategoryTitle,
            //         StringComparer.CurrentCultureIgnoreCase)
            //.ToList();

            //return sections;
        //}
    }
}
