using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs.Playlist;
using Menro.Application.Features.Music.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Music.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/music/playlist")]
    public class PlaylistController : ControllerBase
    {

        #region DI
        private readonly IPlaylistService _playlistService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileUrlService _fileUrlService;
        public PlaylistController(IPlaylistService playlistService,
            ICurrentUserService currentUserService,
            IFileUrlService fileUrlService)
        {
            _playlistService = playlistService;
            _currentUserService = currentUserService;
            _fileUrlService = fileUrlService;
        }
        #endregion


        // add playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlaylistDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var restaurantId = await _currentUserService.GetRestaurantIdAsync();

                var playlist =
                    await _playlistService.CreateAsync(restaurantId, dto);

                return Ok(new
                {
                    playlist.Id,
                    playlist.Name,
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }



        // get all playlists
        [Authorize(Roles = SD.Role_Owner)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var playlists = await _playlistService.GetAllAsync(restaurantId);

            return Ok(playlists);
        }

        // get playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var playlist = await _playlistService.GetByIdAsync(id, restaurantId);

            if (playlist == null)
                return NotFound();

            // url generation
            foreach (var track in playlist.Tracks)
            {
                if (!string.IsNullOrEmpty(track.AudioUrl))
                {
                    track.AudioUrl = _fileUrlService.BuildMusicFileUrl(track.AudioUrl);
                }

                if (!string.IsNullOrEmpty(track.CoverUrl))
                {
                    track.CoverUrl = _fileUrlService.BuildMusicCoverUrl(track.CoverUrl);
                }
            }

            return Ok(playlist);
        }

        // rename playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPut("{playlistId:guid}/rename")]
        public async Task<IActionResult> Rename(Guid playlistId, [FromBody] RenamePlaylistDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var restaurantId = await _currentUserService.GetRestaurantIdAsync();

                var result = await _playlistService.RenameAsync(playlistId, restaurantId, dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "پلی‌لیست یافت نشد."
                    });
                }

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // set active tab on fetch playlists list
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPut("{playlistId:guid}/activate")]
        public async Task<IActionResult> Activate(Guid playlistId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _playlistService.SetActivePlaylistAsync(playlistId, restaurantId);

            if (!result)
                return NotFound();

            return Ok();
        }

        // delete playlist
        [HttpDelete("{playlistId:guid}")]
        public async Task<IActionResult> DeletePlaylist(Guid playlistId)
        {
            int restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _playlistService.DeletePlaylistAsync(
                restaurantId,
                playlistId);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }


        /*-----------------*/
        /* --- Tracks --- */
        /*---------------*/

        // add track to playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPost("{id:guid}/tracks")]
        public async Task<IActionResult> AddTrack(Guid id, [FromBody] AddPlaylistTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _playlistService.AddTrackAsync(
                id,
                restaurantId,
                dto.MusicTrackId
            );

            if (!result)
                return BadRequest("Invalid playlist or track");

            return Ok();
        }



        // remove track from playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpDelete("{id:guid}/tracks/{playlistTrackId:guid}")]
        public async Task<IActionResult> RemoveTrack(Guid id /*(playlistId)*/, Guid playlistTrackId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _playlistService.RemoveTrackAsync(
                id,
                restaurantId,
                playlistTrackId
            );

            if (!result)
                return NotFound();

            return Ok();
        }

        // re-order track in playlist
        [Authorize(Roles = SD.Role_Owner)]
        [HttpPut("{playlistId:guid}/tracks/{playlistTrackId:guid}/move")]
        public async Task<IActionResult> ReorderTrack(Guid playlistId, Guid playlistTrackId, [FromBody] ReorderPlaylistTrackDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _playlistService.ReorderTrackAsync(playlistId, restaurantId, playlistTrackId, dto.Direction);

            if (!result)
                return NotFound();

            return Ok();
        }
    }
}
