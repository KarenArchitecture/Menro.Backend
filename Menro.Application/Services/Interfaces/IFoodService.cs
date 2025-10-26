using Menro.Application.Foods.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IFoodService
    {
        Task<List<FoodsListItemDto>> GetFoodsListAsync(int restaurantId);
        Task<FoodDetailsDto?> GetFoodAsync(int foodId, int restaurantId);
        Task<bool> AddFoodAsync(CreateFoodDto dto, int restaurantId);
        Task<bool> UpdateFoodAsync(UpdateFoodDto dto);
        Task<bool> DeleteFoodAsync(int foodId);

    }
}
