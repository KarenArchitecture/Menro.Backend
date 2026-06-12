using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Music.DTOs;
using Menro.Application.Features.Music.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel.Music
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


        // get all playlists
        [Authorize(Roles = SD.Role_Owner)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var playlists = await _playlistService
                .GetAllAsync(restaurantId);

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
        public async Task<IActionResult> RemoveTrack(Guid id, Guid playlistTrackId)
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
    }
}
