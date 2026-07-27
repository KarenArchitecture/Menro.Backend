using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Public;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Public
{
    [ApiController]
    [Authorize]
    [Route("api/public/music")]
    public class PublicPlaylistController : ApiControllerBase
    {
        #region DI
        private readonly IPublicMusicService _publicMusicService;
        private readonly ICurrentUserService _currentUserService;

        public PublicPlaylistController(
            IPublicMusicService publicMusicService,
            ICurrentUserService currentUserService)
        {
            _publicMusicService = publicMusicService;
            _currentUserService = currentUserService;
        }
        #endregion

        [HttpGet("{restaurantId:int}")]
        public async Task<IActionResult> Get(int restaurantId)
        {
            var userId = _currentUserService.GetUserId();

            var result = await _publicMusicService.GetPageAsync(restaurantId, userId!);

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [Authorize]
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
