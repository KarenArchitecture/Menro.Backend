using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs;
using Menro.Application.Features.Music.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [Authorize]
    [ApiController]
    [Route("api/music-tracks")]
    public class MusicTracksController : ControllerBase
    {
        #region DI
        private readonly IMusicTrackService _musicTrackService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public MusicTracksController(
            IMusicTrackService musicTrackService,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _musicTrackService = musicTrackService;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }
        #endregion


        // add music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPost("add")]
        public async Task<IActionResult> AddAsync(
            [FromBody] CreateMusicTrackDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var restaurantId =
                await _currentUserService.GetRestaurantIdAsync();

            var result =
                await _musicTrackService.CreateAsync(
                    restaurantId,
                    dto);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "خطای ناشناخته‌ای رخ داده است."
                });
            }

            return Ok();
        }

        // get musics
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var tracks = await _musicTrackService
                .GetAllAsync(restaurantId);

            return Ok(tracks);
        }

        // get music
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var track = await _musicTrackService.GetByIdAsync(id, restaurantId);

            if (track == null)
                return NotFound();

            return Ok(track);
        }

        // delete music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var restaurantId =
                await _currentUserService.GetRestaurantIdAsync();

            var track = await _musicTrackService.RemoveAsync(id, restaurantId);

            if (track == null)
            {
                return NotFound(new
                {
                    message = "موسیقی مورد نظر یافت نشد."
                });
            }

            if (!string.IsNullOrWhiteSpace(track.AudioFileName))
            {
                _fileService.DeleteMusic(track.AudioFileName);
            }

            if (!string.IsNullOrWhiteSpace(track.CoverFileName))
            {
                _fileService.DeleteMusicCover(track.CoverFileName);
            }

            return Ok();
        }

        // update music
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateMusicTrackDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var restaurantId =
                await _currentUserService.GetRestaurantIdAsync();

            var result = await _musicTrackService.UpdateAsync(id, restaurantId, dto);

            if (!result)
            {
                return NotFound(new
                {
                    message = "موسیقی مورد نظر یافت نشد."
                });
            }

            return Ok();
        }

        /*--------------http context----------------*/

        /*upload files*/
        [HttpPost("upload-music")]
        [Authorize(Roles = SD.Role_Owner)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMusic(
            [FromForm] UploadMusicDto dto)
        {
            var file = dto.File;

            if (file == null || file.Length == 0)
                return BadRequest("هیچ فایلی ارسال نشده است.");

            var allowedExtensions = new[]
            {
                ".mp3",
                ".wav",
                ".ogg"
            };

            var ext = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("فرمت فایل مجاز نیست.");

            if (file.Length > 20 * 1024 * 1024)
                return BadRequest("حجم فایل بیش از حد مجاز است.");

            try
            {
                var fileName =
                    await _fileService.UploadMusicAsync(file);

                return Ok(new
                {
                    fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "خطا در ذخیره فایل",
                    error = ex.Message
                });
            }
        }

        [HttpPost("upload-cover")]
        [Authorize(Roles = SD.Role_Owner)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCover(
    [FromForm] UploadMusicCoverDto dto)
        {
            var file = dto.File;

            if (file == null || file.Length == 0)
                return BadRequest("هیچ فایلی ارسال نشده است.");

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var ext = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("فرمت فایل مجاز نیست.");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("حجم فایل بیش از 2 مگابایت است.");

            try
            {
                var fileName =
                    await _fileService.UploadMusicCoverAsync(file);

                return Ok(new
                {
                    fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "خطا در ذخیره فایل",
                    error = ex.Message
                });
            }
        }
    }
}
