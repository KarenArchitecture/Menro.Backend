using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Identity.Services;
using Menro.Application.Features.Music.DTOs.Public;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Public
{
    [ApiController]
    [Authorize]
    [Route("api/public/music")]
    public class PublicPlaylistController : ControllerBase
    {
        private readonly IPublicMusicService _publicMusicService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileUrlService _fileUrlService;

        public PublicPlaylistController(
            IPublicMusicService publicMusicService,
            ICurrentUserService currentUserService,
            IFileUrlService fileUrlService)
        {
            _publicMusicService = publicMusicService;
            _currentUserService = currentUserService;
            _fileUrlService = fileUrlService;
        }

        [HttpGet("{restaurantId:int}")]
        public async Task<IActionResult> Get(int restaurantId)
        {
            var userId = _currentUserService.GetUserId();

            var result = await _publicMusicService.GetPageAsync(restaurantId, userId!);

            if (result is null)
                return NotFound();

            result.Tracks = result.Tracks
                .Select(t =>
                {
                    t.ImageUrl = string.IsNullOrWhiteSpace(t.ImageUrl) ? null: _fileUrlService.BuildMusicCoverUrl(t.ImageUrl);

                    return t;
                })
                .ToList();

            return Ok(result);
        }

        [HttpPost("{restaurantId:int}/request")]
        public async Task<IActionResult> RequestTrack(int restaurantId,RequestTrackDto dto)
        {
            var userId = _currentUserService.GetUserId();

            var result = await _publicMusicService.RequestTrackAsync(restaurantId, userId!, dto.MusicTrackId);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
    }
}
