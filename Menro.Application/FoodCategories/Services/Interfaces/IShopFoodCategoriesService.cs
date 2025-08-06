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
        Task<IEnumerable<ShopFoodCategoryDto>> GetCategoriesForRestaurantAsync(string restaurantSlug);
    }
}
