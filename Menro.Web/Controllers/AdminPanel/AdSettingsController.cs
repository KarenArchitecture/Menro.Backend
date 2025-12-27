using Menro.Application.Common.SD;
using Menro.Application.Features.Ads.DTOs;
using Menro.Application.Features.Ads.Services;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Owner}")]
    [Route("api/admin/[controller]")]

    public class AdSettingsController : ControllerBase
    {
        private readonly IAdSettingsService _service;
        public AdSettingsController(IAdSettingsService service)
        {
            _service = service;
        }

        // ✅
        [HttpGet]
        [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Owner}")]
        public async Task<IActionResult> Get([FromQuery] AdPlacementType placement)
        {
            var data = await _service.GetActiveSettingsAsync(placement);
            return Ok(data);
        }

        // ✅
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Save([FromBody] List<AdPricingSettingDto> dtos)
        {
            var result = await _service.SaveSettingsAsync(dtos);
            return Ok(new { success = result });
        }
    }
}
