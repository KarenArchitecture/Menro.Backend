using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/Icon")]
    [Authorize(Roles = $"{SD.Role_Owner},{SD.Role_Admin}")]
    public class AdminIconController : ControllerBase
    {
        #region DI
        private readonly IIconService _iconService;
        private readonly IMediaStorageProvider _mediaStorage;

        public AdminIconController(IIconService iconService, IMediaStorageProvider mediaStorage)
        {
            _iconService = iconService;
            _mediaStorage = mediaStorage;
        }
        #endregion

        [HttpGet("read-all")]
        [Authorize(Roles = $"{SD.Role_Owner},{SD.Role_Admin}")]
        public async Task<IActionResult> GetAll()
        {
            var icons = await _iconService.GetAllAsync();
            return Ok(icons);
        }

        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add([FromForm] AddIconDto dto)
        {
            try
            {
                var success = await _iconService.AddAsync(dto.Label, dto.Icon);
                if (!success)
                    return StatusCode(500, new { message = "Failed to add icon record." });

                return Ok(new { message = "Icon added successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _iconService.DeleteAsync(id);
                if (!success)
                    return StatusCode(500, new { message = "Failed to delete icon record" });

                return Ok(new { message = "Icon deleted successfully", id });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}