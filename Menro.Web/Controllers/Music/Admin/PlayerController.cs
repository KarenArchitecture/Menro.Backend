using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/music/player")]
    public class PlayerController : ControllerBase
    {
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly ICurrentUserService _currentUserService;

        public PlayerController(
            IMusicPlayerService musicPlayerService,
            ICurrentUserService currentUserService)
        {
            _musicPlayerService = musicPlayerService;
            _currentUserService = currentUserService;
        }


        // get music player
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var restaurantId =
                await _currentUserService.GetRestaurantIdAsync();

            var player =
                await _musicPlayerService
                    .GetPlayerAsync(restaurantId);

            if (player == null)
                return NotFound();

            return Ok(player);
        }

        // set current playing track
        [HttpPut("current-track")]
        public async Task<IActionResult> SetCurrentTrack([FromBody] UpdateCurrentTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _musicPlayerService.SetCurrentTrackAsync(restaurantId, dto.PlaylistId, dto.PlaylistTrackId);

            if (!result)
                return NotFound();

            return Ok();
        }


        // advance tracks
        [HttpPut("advance")]
        public async Task<IActionResult> Advance([FromBody] AdvanceTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _musicPlayerService.AdvanceTrackAsync(restaurantId, dto.PlaylistTrackId);

            if (!result)
                return NotFound();

            return Ok();
        }


        [HttpPut("previous")]
        public async Task<IActionResult> Previous([FromBody] PreviousTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _musicPlayerService.MoveToPreviousAsync(restaurantId, dto.PlaylistTrackId);

            if (!result)
                return NotFound();

            return Ok();
        }
    }
}
