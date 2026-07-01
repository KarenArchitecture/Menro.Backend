using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Archive;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Music.Services.Implementations
{
    internal class MusicTrackService : IMusicTrackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMusicNotificationService _musicNotificationService;

        public MusicTrackService(IUnitOfWork uow, IMusicNotificationService musicNotificationService)
        {
            _uow = uow;
            _musicNotificationService = musicNotificationService;
        }


        // add music
        public async Task<MusicTrack> CreateAsync(int restaurantId, CreateMusicTrackDto dto)
        {
            var track = new MusicTrack
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,

                Title = dto.Title,
                Artist = dto.Artist,
                Duration = dto.Duration,

                AudioFileName = dto.AudioFileName,
                CoverFileName = dto.CoverFileName,

            };

            await _uow.MusicTrack.AddAsync(track);
            await _uow.SaveChangesAsync();

            return track;
        }

        // get musics
        public async Task<List<MusicTrackListItemDto>> GetAllAsync(int restaurantId)
        {
            var tracks = await _uow.MusicTrack.GetAllByRestaurantIdAsync(restaurantId);

            return tracks.Select(t => new MusicTrackListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Artist = t.Artist,
                Duration = t.Duration,
                CoverFileName = t.CoverFileName,
            }).ToList();
        }


        // get music
        public async Task<MusicTrackDto?> GetByIdAsync(Guid trackId, int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return null;

            return new MusicTrackDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.Artist,
                Duration = track.Duration,

                AudioUrl = track.AudioFileName,
                CoverUrl = track.CoverFileName,
            };
        }

        // remove music
        public async Task<MusicTrack?> RemoveAsync(Guid trackId, int restaurantId)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return null;

            try
            {
                bool playbackChanged = false;

                // 1. get player
                var player = await _uow.MusicPlayer.GetByRestaurantIdAsync(restaurantId);

                // 2. playlist dependencies
                var playlistTracks = await _uow.PlaylistTrack.GetAllByMusicTrackId(trackId);

                // 3. request dependencies
                var trackRequests = await _uow.TrackRequest.GetAllByMusicTrackId(trackId);

                // 4. if is set as current, update player
                if (player?.CurrentPlaylistTrackId != null)
                {
                    var currentPlaylistTrack = playlistTracks.FirstOrDefault(
                        x => x.Id == player.CurrentPlaylistTrackId
                    );

                    if (currentPlaylistTrack != null)
                    {
                        var nextTrack = await _uow.PlaylistTrack.GetNextTrackAsync(
                            currentPlaylistTrack.PlaylistId,
                            currentPlaylistTrack.SortOrder
                        );

                        if (nextTrack != null)
                        {
                            player.CurrentPlaylistTrackId = nextTrack.Id;
                            player.PlaylistId = nextTrack.PlaylistId;
                        }
                        else
                        {
                            player.CurrentPlaylistTrackId = null;
                            player.PlaylistId = null;
                        }

                        player.LastUpdatedAt = DateTime.UtcNow;

                        await _uow.MusicPlayer.UpdateAsync(player);

                        playbackChanged = true;
                    }
                }

                // 5. delete playlistTrack & trackRequest references
                await _uow.PlaylistTrack.RemoveRange(playlistTracks);
                await _uow.TrackRequest.RemoveRange(trackRequests);

                // 6. delete track
                _uow.MusicTrack.Remove(track);

                await _uow.SaveChangesAsync();

                // 7. notify playlist change (only if playlist actually affected)
                if (playlistTracks.Any())
                {
                    await _musicNotificationService.NotifyPlaylistChanged(restaurantId);
                }

                // 8. notify playback change (only if player state changed)
                if (playbackChanged)
                {
                    await _musicNotificationService.NotifyPlaybackChanged(
                        restaurantId,
                        new MusicPlayerDto
                        {
                            PlaylistId = player?.PlaylistId,
                            CurrentTrackId = player?.CurrentPlaylistTrackId
                        });
                }

                return track;
            }
            catch
            {
                return null;
            }
        }

        // rename music track title
        public async Task<bool> UpdateAsync(Guid trackId, int restaurantId, UpdateMusicTrackDto dto)
        {
            var track = await _uow.MusicTrack.GetByIdAsync(
                trackId,
                restaurantId);

            if (track == null)
                return false;

            track.Title = dto.Title;

            await _uow.MusicTrack.UpdateAsync(track);

            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
