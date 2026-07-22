using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs.Playlist;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Owner)]
    [Route("api/admin/music/playlist")]
    public class PlaylistController : ControllerBase
    {
        #region DI
        private readonly IPlaylistService _playlistService;
        private readonly ICurrentUserService _currentUserService;

        public PlaylistController(
            IPlaylistService playlistService,
            ICurrentUserService currentUserService)
        {
            _playlistService = playlistService;
            _currentUserService = currentUserService;
        }
        #endregion

        // add playlist
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlaylistDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var restaurantId = await _currentUserService.GetRestaurantIdAsync();
                var playlist = await _playlistService.CreateAsync(restaurantId, dto);

                return Ok(new { playlist.Id, playlist.Name });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // get all playlists
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var playlists = await _playlistService.GetAllAsync(restaurantId);
            return Ok(playlists);
        }

        // get playlist
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var playlist = await _playlistService.GetByIdAsync(id, restaurantId);

            return playlist == null ? NotFound() : Ok(playlist);
        }

        // rename playlist
        [HttpPut("{playlistId:guid}/rename")]
        public async Task<IActionResult> Rename(Guid playlistId, [FromBody] RenamePlaylistDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var restaurantId = await _currentUserService.GetRestaurantIdAsync();
                var result = await _playlistService.RenameAsync(playlistId, restaurantId, dto);

                return result ? Ok() : NotFound(new { message = "پلی‌لیست یافت نشد." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // set active tab on fetch playlists list
        [HttpPut("{playlistId:guid}/activate")]
        public async Task<IActionResult> Activate(Guid playlistId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _playlistService.SetActivePlaylistAsync(playlistId, restaurantId);

            return result ? Ok() : NotFound();
        }

        // delete playlist
        [HttpDelete("{playlistId:guid}")]
        public async Task<IActionResult> DeletePlaylist(Guid playlistId)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _playlistService.DeletePlaylistAsync(restaurantId, playlistId);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /*-----------------*/
        /* --- Tracks --- */
        /*---------------*/

        [HttpPost("{id:guid}/tracks")]
        public async Task<IActionResult> AddTrack(Guid id, [FromBody] AddPlaylistTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _playlistService.AddTrackAsync(id, restaurantId, dto.MusicTrackId);

            return result ? Ok() : BadRequest("Invalid playlist or track");
        }

        [HttpDelete("{id:guid}/tracks/{playlistTrackId:guid}")]
        public async Task<IActionResult> RemoveTrack(Guid id, Guid playlistTrackId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _playlistService.RemoveTrackAsync(id, restaurantId, playlistTrackId);

            return result ? Ok() : NotFound();
        }

        [HttpPut("{playlistId:guid}/tracks/{playlistTrackId:guid}/move")]
        public async Task<IActionResult> ReorderTrack(Guid playlistId, Guid playlistTrackId, [FromBody] ReorderPlaylistTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            var result = await _playlistService.ReorderTrackAsync(playlistId, restaurantId, playlistTrackId, dto.Direction);

            return result ? Ok() : NotFound();
        }
    }
}