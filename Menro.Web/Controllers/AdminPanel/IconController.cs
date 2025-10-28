using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IconController : ControllerBase
    {
        private readonly IIconService _iconService;

        public IconController(IIconService iconService)
        {
            _iconService = iconService;
        }

        // ✅
        [HttpGet("read-all")]
        [Authorize(Roles = $"{SD.Role_Owner},{SD.Role_Admin}")]
        public async Task<IActionResult> GetAll()
        {
            var icons = await _iconService.GetAllAsync();
            return Ok(icons);
        }

        // ✅
        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add([FromBody] AddIconDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _iconService.AddAsync(dto);

                if (!success)
                    return StatusCode(500, new { message = "Failed to add icon record." });

                return Ok(new { message = "Icon added successfully.", fileName = dto.FileName });
            }
            catch (InvalidOperationException ex)
            {
                // در صورت تکراری بودن فایل
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // در صورت نامعتبر بودن داده‌ها
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // خطاهای غیرمنتظره
                return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _iconService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Icon not found" });

            var result = await _iconService.DeleteAsync(id);
            if (!result)
                return StatusCode(500, new { message = "Failed to delete icon record" });

            return Ok(new { message = "Icon record deleted successfully", id });
        }

    }
}
