using Menro.Application.Features.AdminPanel.Dashboard;
using Menro.Application.Restaurants.Services.Implementations;
using Menro.Application.Restaurants.Services.Interfaces;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Security.Claims;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize(Roles = "Owner,Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var total = await _dashboardService.GetTotalRevenueAsync();
            return Ok(total);
        }

        [HttpGet("new-orders")]
        public async Task<IActionResult> NewOrdersCount([FromQuery] int? restaurantId)
        {
            int count = await _dashboardService.GetNewOrdersCountAsync(restaurantId);
            return Ok(count);
        }
        
        // GET /api/adminpanel/dashboard/monthly-sales?restaurantId=3
        [HttpGet("monthly-sales")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetMonthlySales()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? restaurantId = await _dashboardService.GetRestaurantIdByUserIdAsync(userId);
            var data = await _dashboardService.GetMonthlySalesAsync(restaurantId);
            return Ok(data); // [{month:1,totalSales:...}, ...]
        }

        [HttpGet("restaurant-id")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetRestaurantId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? restaurantId = await _dashboardService.GetRestaurantIdByUserIdAsync(userId);
            return Ok(new { restaurantId });
        }


    }

}
