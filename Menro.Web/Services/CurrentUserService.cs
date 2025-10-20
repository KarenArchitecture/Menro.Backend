using Menro.Application.Features.AdminPanel.Services;
using Menro.Application.Features.Identity.Services;
using System.Security.Claims;

namespace Menro.Web.Services
{
    // Menro.Web.Services
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDashboardService _dashboardService;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor,
                                  IDashboardService dashboardService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dashboardService = dashboardService;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<int> GetRestaurantIdAsync()
        {
            var userId = GetUserId();
            return await _dashboardService.GetRestaurantIdByUserIdAsync(userId);
        }
    }

}
