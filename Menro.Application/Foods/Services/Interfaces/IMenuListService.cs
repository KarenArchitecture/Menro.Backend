using Menro.Application.Foods.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Foods.Services.Interfaces
{
    public interface IMenuListService
    {
        //Task<List<MenuListFoodDto>> GetMenuListAsync(
        //    int restaurantId,
        //    int? globalCategoryId = null,
        //    int? customCategoryId = null);

        Task<List<MenuListFoodDto>> GetMenuListBySlugAsync(
            string restaurantSlug,
            int? globalCategoryId = null,
            int? customCategoryId = null);
    }
}