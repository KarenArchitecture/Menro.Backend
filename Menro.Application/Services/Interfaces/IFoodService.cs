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
        Task<FoodDetailsDto> CreateFoodAsync(CreateFoodDto dto, int restaurantId);
        Task<bool> DeleteFoodAsync(int foodId);

    }
}
