using Menro.Application.Common.Interfaces;
using Menro.Application.Features.AdminPanel.Services;
using Menro.Application.Restaurants.Services.Interfaces;
using System.Security.Claims;

namespace Menro.Web.Services
{
    // Menro.Web.Services
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRestaurantService _restaurantService;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor,
                                  IRestaurantService restaurantService)
        {
            _httpContextAccessor = httpContextAccessor;
            _restaurantService = restaurantService;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<int> GetRestaurantIdAsync()
        {
            var userId = GetUserId();
            return await _restaurantService.GetRestaurantIdByUserIdAsync(userId);
        }
    }

}
