using Menro.Application.Common.Interfaces;
using Menro.Application.Features.AdminPanel.DTOs;
using Menro.Application.Features.Identity.Services;
using Menro.Application.Features.Order.Services;
using Menro.Application.Restaurants.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Menro.Application.Features.AdminPanel.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardService(IOrderService orderService, 
            ICurrentUserService currentUserService)
        {
            _orderService = orderService;
            _currentUserService = currentUserService;
        }

        // all dashboard data for the current user (Owner/Admin)
        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId is null)
            {
                restaurantId = 0;
            }

            // 📍 1. restaurant name
            //var restaurant = await _restaurantService.GetRestaurantName(restaurantId.Value);

            // 📍 2. finance numbers
            int todayOrdersCount = await _orderService.GetRecentOrdersCountAsync(restaurantId, 0);
            decimal todayRevenue = await _orderService.GetRecentOrdersRevenueAsync(restaurantId, 0);
            decimal totalRevenue = await _orderService.GetTotalRevenueAsync(restaurantId);

            // 📍 3. monthly sales
            var monthlySalesRaw = await _orderService.GetMonthlySalesRawAsync(restaurantId);
            var monthlySales = monthlySalesRaw.Select(r => new SalesByMonthDto
            {
                Month = r.Month,
                MonthName = r.MonthName,
                TotalSales = r.TotalAmount
            }).ToList();

            var dto = new DashboardDto
            {
                //RestaurantName = restaurant ?? string.Empty,
                TodayOrdersCount = todayOrdersCount,
                TodayRevenue = todayRevenue,
                TotalRevenue = totalRevenue,
                MonthlySales = monthlySales
            };

            return dto;
        }


        // financial statistics
        public async Task<decimal> GetTotalRevenueAsync(int? restaurantId = null)
        {
            return await _orderService.GetTotalRevenueAsync(restaurantId);
        }
        public async Task<int> GetThisMonthOrdersCountAsync(int? restaurantId = null)
        {
            return await _orderService.GetRecentOrdersCountAsync(restaurantId, 30);
        }
        public async Task<int> GetTodayOrdersCountAsync(int? restaurantId = null)
        {
            return await _orderService.GetRecentOrdersCountAsync(restaurantId, 0); // daysBack=0 یعنی فقط امروز
        }
        public async Task<decimal> GetTodayOrdersRevenueAsync(int? restaurantId = null)
        {
            return await _orderService.GetRecentOrdersRevenueAsync(restaurantId, 0);
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

    }
}
