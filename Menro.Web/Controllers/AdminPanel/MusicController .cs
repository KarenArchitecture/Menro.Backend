using Menro.Application.Common.Interfaces;
using Menro.Application.Features.MusicPlayer.Dtos;
using Menro.Application.Features.MusicPlayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/admin/music")]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;
        private readonly IFileService _fileService;

        public MusicController(
            IMusicService musicService,
            IFileService fileService)
        {
            _musicService = musicService;
            _fileService = fileService;
        }

        // -------------------------
        // GET: api/admin/music
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string? search)
        {
            var result = await _musicService.GetListAsync(search);
            return Ok(result);
        }

        // -------------------------
        // GET: api/admin/music/{id}
        // -------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _musicService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // -------------------------
        // POST: api/admin/music
        // -------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMusicDto dto)
        {
            if (dto.MusicFile == null)
                return BadRequest("Music file is required");

            // آپلود فایل اصلی موزیک
            var musicFilePath = await _fileService.UploadMusicAsync(dto.MusicFile);
            // ⬆️ فعلاً از همین استفاده کردی
            // بعداً بهتره UploadMusicAsync اضافه بشه

            string? coverPath = null;
            if (dto.CoverFile != null)
            {
                coverPath = await _fileService.UploadMusicCoverAsync(dto.CoverFile);
            }

            await _musicService.CreateAsync(dto, musicFilePath, coverPath);

            return Ok();
        }

        // -------------------------
        // PUT: api/admin/music
        // -------------------------
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMusicDto dto)
        {
            await _musicService.UpdateAsync(dto);
            return Ok();
        }

        // -------------------------
        // DELETE: api/admin/music/{id}
        // -------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _musicService.DeleteAsync(id);
            return Ok();
        }
    }
}
