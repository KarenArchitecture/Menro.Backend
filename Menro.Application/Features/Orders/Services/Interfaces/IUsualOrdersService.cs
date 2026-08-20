using Menro.Application.Features.Orders.DTOs;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IUsualOrdersService
    {
        Task<List<UsualOrderFoodDto>> GetUsualFoodsAsync(string userId, int restaurantId, int count = 12);
    }
}