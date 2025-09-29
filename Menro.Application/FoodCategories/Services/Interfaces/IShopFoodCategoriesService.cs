using Menro.Application.FoodCategories.DTOs;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.FoodCategories.Services.Interfaces
{
    public interface IShopFoodCategoriesService
    {
        /// <summary>
        /// Get all categories specific to a restaurant
        /// </summary>
        Task<List<ShopFoodCategoryDto>> GetByRestaurantAsync(int restaurantId);

        /// <summary>
        /// Get all global categories available to all restaurants
        /// </summary>
        Task<List<ShopFoodCategoryDto>> GetGlobalCategoriesAsync();

        //// <summary>
        /// Get all food categories (global + restaurant-specific) for shop page by restaurant slug
        /// </summary>
        //Task<List<ShopFoodCategoryDto>> GetAllForRestaurantAsync(string restaurantSlug);

    }
}
