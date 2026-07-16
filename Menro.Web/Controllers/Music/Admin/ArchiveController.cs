using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs.Archive;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Music;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/music/archive")]
    public class ArchiveController : ControllerBase
    {
        #region DI
        private readonly IMusicTrackService _musicTrackService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;

        public ArchiveController(
            IMusicTrackService musicTrackService,
            ICurrentUserService currentUserService,
            IFileService fileService,
            IFileUrlService fileUrlService)
        {
            _musicTrackService = musicTrackService;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _fileUrlService = fileUrlService;
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

            if (dto.AudioFile == null || dto.AudioFile.Length == 0)
                return BadRequest("فایل موسیقی ارسال نشده است.");

            string? audioFileName = null;
            string? coverFileName = null;

            try
            {
                // ---------------- AUDIO ----------------
                var allowedAudioExtensions = new[] { ".mp3", ".wav", ".ogg" };

                var audioExt = Path.GetExtension(dto.AudioFile.FileName)
                    .ToLowerInvariant();

                if (!allowedAudioExtensions.Contains(audioExt))
                    return BadRequest("فرمت فایل موسیقی مجاز نیست.");

                if (dto.AudioFile.Length > 20 * 1024 * 1024)
                    return BadRequest("حجم فایل موسیقی بیش از حد مجاز است.");

                audioFileName = await _fileService.UploadMusicAsync(dto.AudioFile);

                var audioPath = _fileService.GetMusicPhysicalPath(audioFileName);

                var metadata = AudioMetadataExtractor.Extract(audioPath);

                // ---------------- COVER ----------------
                if (dto.CoverFile != null)
                {
                    var allowedCoverExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    var coverExt = Path.GetExtension(dto.CoverFile.FileName)
                        .ToLowerInvariant();

                    if (!allowedCoverExtensions.Contains(coverExt))
                        return BadRequest("فرمت تصویر کاور مجاز نیست.");

                    if (dto.CoverFile.Length > 2 * 1024 * 1024)
                        return BadRequest("حجم تصویر کاور بیش از 2 مگابایت است.");

                    coverFileName = await _fileService.UploadMusicCoverAsync(dto.CoverFile);
                }
                else
                {
                    var coverBytes = AudioMetadataExtractor.ExtractCover(audioPath);

                    if (coverBytes != null)
                    {
                        coverFileName =
                            await _fileService.SaveMusicCoverFromBytesAsync(coverBytes);
                    }
                }

                // ---------------- CREATE DTO ----------------
                var createDto = new CreateMusicTrackDto
                {
                    Title = metadata.Title,
                    Artist = metadata.Artist,
                    Duration = metadata.Duration,
                    AudioFileName = audioFileName,
                    CoverFileName = coverFileName
                };

                var restaurantId = await _currentUserService.GetRestaurantIdAsync();

                MusicTrack result;

                try
                {
                    result = await _musicTrackService.CreateAsync(restaurantId, createDto);
                }
                catch
                {
                    // rollback DB failure → cleanup files
                    if (!string.IsNullOrWhiteSpace(audioFileName))
                        _fileService.DeleteMusic(audioFileName);

                    if (!string.IsNullOrWhiteSpace(coverFileName))
                        _fileService.DeleteMusicCover(coverFileName);

                    throw;
                }

                // ---------------- RESPONSE ----------------
                return Ok(new MusicTrackListItemDto
                {
                    Id = result.Id,
                    Title = result.Title,
                    Artist = result.Artist,
                    Duration = result.Duration,

                    CoverFileName = coverFileName == null
                        ? null
                        : _fileUrlService.BuildMusicCoverUrl(coverFileName)
                });
            }
            catch (Exception ex)
            {
                // global cleanup (file system rollback safety)
                if (!string.IsNullOrWhiteSpace(audioFileName))
                    _fileService.DeleteMusic(audioFileName);

                if (!string.IsNullOrWhiteSpace(coverFileName))
                    _fileService.DeleteMusicCover(coverFileName);

                return StatusCode(500, new
                {
                    message = "خطا در ذخیره موسیقی",
                    error = ex.Message
                });
            }
        }


        // get musics
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var tracks = await _musicTrackService.GetAllAsync(restaurantId);

            var result = tracks.Select(t => new MusicTrackListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Artist = t.Artist,
                Duration = t.Duration,

                CoverFileName = string.IsNullOrWhiteSpace(t.CoverFileName)
                    ? null
                    : _fileUrlService.BuildMusicCoverUrl(t.CoverFileName)
            });

            return Ok(result);
        }

        // get (play) music
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var track = await _musicTrackService.GetByIdAsync(id, restaurantId);

            if (track == null)
                return NotFound();

            var result = new MusicTrackDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.Artist,
                Duration = track.Duration,

                AudioUrl = _fileUrlService.BuildMusicFileUrl(track.AudioUrl),
                CoverUrl = string.IsNullOrWhiteSpace(track.AudioUrl) ? null : _fileUrlService.BuildMusicCoverUrl(track.AudioUrl)
            };

            return Ok(result);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMusicTrackDto dto)
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
    }
}
