using Menro.Application.Common.SD;
using Menro.Application.Features.AdminPanel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/adminpanel/[controller]")]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Owner}")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ✅
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetAdminDetails()
        {
            var adminDetails = await _dashboardService.GetDashboardDataAsync();
            return Ok(adminDetails);
        }

    }

}
