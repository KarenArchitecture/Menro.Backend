using Menro.Application.DTO;
using Menro.Application.Restaurants.DTOs;
using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Restaurants.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> AddRestaurantAsync(RegisterRestaurantDto dto, string ownerUserId);
        Task<List<RestaurantCategoryDto>> GetRestaurantCategoriesAsync();
        Task<string> GenerateUniqueSlugAsync(string name);


    }
}
