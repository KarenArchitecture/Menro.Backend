using Menro.Application.Features.Orders.DTOs;

namespace Menro.Application.Features.Orders.Services.Interfaces
{
    public interface IUserRecentOrderCardService
    {
        Task<List<RecentOrdersFoodCardDto>> GetUserRecentOrderedFoodsAsync(string userId, int count = 8);
    }
}
