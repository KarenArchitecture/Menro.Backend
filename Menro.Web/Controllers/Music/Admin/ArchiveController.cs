using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs.Archive;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/music/archive")]
    public class ArchiveController : ApiControllerBase
    {
        #region DI
        private readonly IMusicTrackService _musicTrackService;
        private readonly ICurrentUserService _currentUserService;

        public ArchiveController(
            IMusicTrackService musicTrackService,
            ICurrentUserService currentUserService)
        {
            _musicTrackService = musicTrackService;
            _currentUserService = currentUserService;
        }
        #endregion

        // add music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] UploadMusicTrackDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var restaurantId = await _currentUserService.GetRestaurantIdAsync();
                var result = await _musicTrackService.CreateAsync(restaurantId, dto.AudioFile, dto.CoverFile);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // شامل خطاهای فرمت/سایز نامعتبر که MediaCategoryRegistry تولید می‌کنه
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در ذخیره موسیقی", error = ex.Message });
            }
        }

        // get musics
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var tracks = await _musicTrackService.GetAllAsync(restaurantId);
            return Ok(tracks);
        }

        // get music metadata (NOT the audio bytes - use /stream for that)
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var track = await _musicTrackService.GetByIdAsync(id, restaurantId);

            return track == null ? NotFound() : Ok(track);
        }

        // stream (play) the actual audio bytes - protected, not a static file
        [Authorize]
        [HttpGet("{id:guid}/stream")]
        public async Task<IActionResult> Stream(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var path = await _musicTrackService.GetAudioPhysicalPathAsync(id, restaurantId);

            if (path == null || !System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, "audio/mpeg", enableRangeProcessing: true);
        }

        // delete music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var track = await _musicTrackService.RemoveAsync(id, restaurantId);

            if (track == null)
                return NotFound(new { message = "موسیقی مورد نظر یافت نشد." });

            return Ok();
        }

        // update music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMusicTrackDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _musicTrackService.UpdateAsync(id, restaurantId, dto);

            return result ? Ok() : NotFound(new { message = "موسیقی مورد نظر یافت نشد." });
        }
    }
}