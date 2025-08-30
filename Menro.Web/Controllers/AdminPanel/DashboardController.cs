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
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        // hepler for getting restaurant Id by userId
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        private async Task<int?> GetRestaurantIdAsync()
        {
            string userId = GetUserId()!;
            int? restaurantId = await _dashboardService.GetRestaurantIdByUserIdAsync(userId);
            return restaurantId;
        }

        /* controller endpoints */

        [HttpGet("admin-details")]
        [Authorize]
        public async Task<IActionResult> GetAdminDetails()
        {
            var adminDetails = await _dashboardService.GetAdminDetailsAsync(GetUserId()!);
            return Ok(adminDetails);
        }
        [HttpGet("restaurant-id")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetRestaurantId()
        {
            string? userId = GetUserId();
            int? restaurantId = await GetRestaurantIdAsync();
            return Ok(new { restaurantId });
        }


        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var total = await _dashboardService.GetTotalRevenueAsync(await GetRestaurantIdAsync());
            return Ok(total);
        }

        [HttpGet("new-orders")]
        public async Task<IActionResult> GetNewOrdersCount()
        {
            int count = await _dashboardService.GetNewOrdersCountAsync(await GetRestaurantIdAsync());
            return Ok(count);
        }
        
        [HttpGet("monthly-sales")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetMonthlySales()
        {
            int? restaurantId = await GetRestaurantIdAsync();
            var data = await _dashboardService.GetMonthlySalesAsync(restaurantId);
            return Ok(data); // [{month:1,totalSales:...}, ...]
        }

    }

}
