using Menro.Application.Common.SD;
using Menro.Application.Features.SiteContent.DTOs;
using Menro.Application.Features.SiteContent.Services.Interfaces;
using Menro.Domain.Entities.SiteContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.SiteContent
{
    [ApiController]
    [Route("api/admin/site-links")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminSiteLinksController : ApiControllerBase
    {
        private readonly ISiteLinkService _menuItemService;

        public AdminSiteLinksController(ISiteLinkService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        /// <summary>همه‌ی آیتم‌های همه‌ی منوها (فعال و غیرفعال).</summary>
        [HttpGet]
        public async Task<ActionResult<List<SiteLinkDto>>> GetAll()
        {
            var result = await _menuItemService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>آیتم‌های یک منوی خاص برای نمایش در پنل ادمین.</summary>
        [HttpGet("{location}")]
        public async Task<ActionResult<List<SiteLinkDto>>> GetByLocation(MenuLocation location)
        {
            var result = await _menuItemService.GetAdminMenuAsync(location);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<SiteLinkDto>> Create([FromBody] CreateSiteLinkDto dto)
        {
            var result = await _menuItemService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByLocation), new { location = result.Location }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SiteLinkDto>> Update(Guid id, [FromBody] UpdateSiteLinkDto dto)
        {
            try
            {
                var result = await _menuItemService.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _menuItemService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>ترتیب جدید آیتم‌های یک منو (بعد از درگ‌اند‌دراپ در پنل).</summary>
        [HttpPut("{location}/reorder")]
        public async Task<IActionResult> Reorder(MenuLocation location, [FromBody] ReorderSiteLinkDto dto)
        {
            await _menuItemService.ReorderAsync(location, dto);
            return NoContent();
        }
    }
}