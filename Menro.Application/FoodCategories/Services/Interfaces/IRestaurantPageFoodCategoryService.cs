using Menro.Application.FoodCategories.DTOs;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Interfaces
{
    public interface IRestaurantPageFoodCategoryService
    {
        /// <summary>
        /// Returns all food categories (global + custom) for a restaurant identified by its slug.
        /// </summary>
        Task<List<RestaurantFoodCategoryDto>> GetCategoriesByRestaurantSlugAsync(string restaurantSlug);
    }
}
