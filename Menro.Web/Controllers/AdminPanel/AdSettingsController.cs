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

        // GET: /api/admin/ad-pricing-settings?placement=MainSlider
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Get([FromQuery] AdPlacementType placement)
        {
            var data = await _service.GetActiveSettingsAsync(placement);
            return Ok(data);
        }

        // POST: /api/admin/ad-pricing-settings
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Save([FromBody] List<AdPricingSettingDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { success = false, error = "Settings list is empty." });

            try
            {
                await _service.SaveSettingsAsync(dtos);
                return Ok(new { success = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
