namespace Menro.Application.Common.Interfaces
{
    public interface IRestaurantAdminAccessService
    {
        Task<bool> IsAdminOfRestaurantAsync(string userId, int restaurantId);
    }
}