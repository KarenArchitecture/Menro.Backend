using Menro.Application.Features.AdminPanel.DTOs;
using Menro.Application.Features.Identity.Services;
using Menro.Application.Features.Order.Services;
using Menro.Application.Restaurants.Services.Interfaces;

namespace Menro.Application.Features.AdminPanel.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IRestaurantService _restaurantService;

        public DashboardService(IUserService userService, IOrderService orderService, IRestaurantService restaurantService)
        {
            _userService = userService;
            _orderService = orderService;
            _restaurantService = restaurantService;
        }
        public async Task<AdminDto> GetAdminDetailsAsync(string userId)
        {
            var user = await _userService.GetByIdAsync(userId);

            int? restaurantId = await _restaurantService.GetRestaurantIdByUserIdAsync(userId);

            string restaurantName = "منرو"; // fallback
            if (restaurantId is not null)
            {
                restaurantName = await _restaurantService.GetRestaurantName(restaurantId!.Value);
            }

            return new AdminDto
            {
                UserFullName = user?.FullName?? user?.UserName!,
                RestaurantName = restaurantName
            };
        }

        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            return await _orderService.GetTotalRevenueAsync(restaurantId);
        }
        public async Task<int> GetNewOrdersCountAsync(int? restaurantId = null)
        {
            // می‌تونه default daysBack خودش رو هم override کنه یا ثابت بذاره
            return await _orderService.GetNewOrdersCountAsync(restaurantId, 23);
        }
        public async Task<List<SalesByMonthDto>> GetMonthlySalesAsync(int? restaurantId)
        {
            var rawSales = await _orderService.GetMonthlySalesRawAsync(restaurantId);
            return rawSales.Select(r => new SalesByMonthDto
            {
                Month = r.Month,
                TotalSales = r.TotalAmount
            }).ToList();
        }
        public async Task<int> GetRestaurantIdByUserIdAsync(string userId)
        {
            int restaurantId = await _restaurantService.GetRestaurantIdByUserIdAsync(userId);
            return restaurantId;
        }

    }
}
