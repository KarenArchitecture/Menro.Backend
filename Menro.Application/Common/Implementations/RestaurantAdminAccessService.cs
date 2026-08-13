using Menro.Application.Common.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Common.Implementations
{
    public class RestaurantAdminAccessService : IRestaurantAdminAccessService
    {
        private readonly IRestaurantRepository _repository;

        public RestaurantAdminAccessService(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsAdminOfRestaurantAsync(string userId, int restaurantId)
        {
            return await _repository.IsAdminOfRestaurantAsync(userId, restaurantId);
            throw new NotImplementedException(
                "باید بر اساس ساختار واقعی رابطه‌ی ادمین-رستوران پیاده بشه");
        }
    }
}