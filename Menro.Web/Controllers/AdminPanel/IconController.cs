using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Menro.Application.Common.SD;
using Menro.Application.Common.Interfaces;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{SD.Role_Owner},{SD.Role_Admin}")]
    public class IconController : ControllerBase
    {
        #region DI
        private readonly IIconService _iconService;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;
        public IconController(IIconService iconService, IFileService fileService, IFileUrlService fileUrlService)
        {
            _iconService = iconService;
            _fileService = fileService;
            _fileUrlService = fileUrlService;
        }

        #endregion


        // ✅
        [HttpGet("read-all")]
        [Authorize(Roles = $"{SD.Role_Owner},{SD.Role_Admin}")]
        public async Task<IActionResult> GetAll()
        {

            var icons = await _iconService.GetAllAsync();
            icons.ForEach(item =>
            {
                item.Url = _fileUrlService.BuildIconUrl(item.Url);
            });
            return Ok(icons);
        }

        // ✅
        [HttpPost("add")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add([FromForm] AddIconDto dto)
        {
            // validation
            if (dto.Icon == null || dto.Icon.Length == 0)
                return BadRequest(new { message = "File is required." });

            if (!dto.Icon.FileName.ToLower().EndsWith(".svg"))
                return BadRequest(new { message = "Only .svg files are allowed." });

            // operation
            try
            {
                string fileName = await _fileService.UploadSvgAsync(dto.Icon);
                var success = await _iconService.AddAsync(dto.Label, fileName);

                if (!success)
                    return StatusCode(500, new { message = "Failed to add icon record." });

                return Ok(new { message = "Icon added successfully.", fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
            }
        }

        // ✅
        [HttpDelete("delete")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _iconService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Icon not found" });
            existing.Url = _fileUrlService.BuildIconUrl(existing.Url);
            // 1. Remove database record
            var dbResult = await _iconService.DeleteAsync(id);

            if (!dbResult)
                return StatusCode(500, new { message = "Failed to delete icon record" });

            // 2. Remove physical file
            var fileRemoved = _fileService.DeleteIcon(existing.FileName);

            return Ok(new
            {
                message = "Icon deleted successfully",
                deletedFromDb = true,
                deletedFile = fileRemoved,
                id
            });
        }

    }
}
