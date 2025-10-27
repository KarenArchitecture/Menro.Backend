using Menro.Application.Common.Interfaces;
using Menro.Application.Features.AdminPanel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet("admin-details")]
        [Authorize]
        public async Task<IActionResult> GetAdminDetails()
        {
            var adminDetails = await _dashboardService.GetAdminDetailsAsync(_currentUserService.GetUserId()!);
            return Ok(adminDetails);
        }

        [HttpGet("restaurant-id")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetRestaurantId()
        {
            string? userId = _currentUserService.GetUserId();
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            return Ok(new { restaurantId });
        }


        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var total = await _dashboardService.GetTotalRevenueAsync(await _currentUserService.GetRestaurantIdAsync());
            return Ok(total);
        }

        [HttpGet("new-orders")]
        public async Task<IActionResult> GetNewOrdersCount()
        {
            int count = await _dashboardService.GetNewOrdersCountAsync(await _currentUserService.GetRestaurantIdAsync());
            return Ok(count);
        }
        
        [HttpGet("monthly-sales")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetMonthlySales()
        {
            int? restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var data = await _dashboardService.GetMonthlySalesAsync(restaurantId);
            return Ok(data); // [{month:1,totalSales:...}, ...]
        }

    }

}
