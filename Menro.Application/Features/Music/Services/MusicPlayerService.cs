using Menro.Application.Features.Music.DTOs.Player;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;

namespace Menro.Application.Features.Music
{
    public class MusicPlayerService : IMusicPlayerService
    {
        private readonly IMusicPlayerRepository _musicPlayerRepository;
        private readonly IPlaylistTrackRepository _playlistTrackRepository;
        public MusicPlayerService(IMusicPlayerRepository musicPlayerRepository, 
            IPlaylistTrackRepository playlistTrackRepository)
        {
            _musicPlayerRepository = musicPlayerRepository;
            _playlistTrackRepository = playlistTrackRepository;
        }

        public async Task<MusicPlayerDto?> GetPlayerAsync(int restaurantId)
        {
            var player = await _musicPlayerRepository.GetByRestaurantIdAsync(restaurantId);

            if (player == null)
                return null;

            return new MusicPlayerDto
            {
                PlaylistId = player.PlaylistId,
                CurrentPlaylistTrackId = player.CurrentPlaylistTrackId,
                LastUpdatedAt = player.LastUpdatedAt
            };
        }


        public async Task<bool> SetCurrentTrackAsync(int restaurantId, Guid playlistId, Guid playlistTrackId)
        {
            var track =
                await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        public async Task<bool> AdvanceTrackAsync(int restaurantId, Guid playlistTrackId)
        {
            var track =
                await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        public async Task<bool> MoveToPreviousAsync(int restaurantId, Guid playlistTrackId)
        {
            var track =
                await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        private async Task<bool> ChangeTrackAsync(int restaurantId, PlaylistTrack newTrack)
        {
            var player = await _musicPlayerRepository.GetByRestaurantIdAsync(restaurantId);

            if (player == null)
                return false;

            PlaylistTrack? currentTrack = null;
            if (player.CurrentPlaylistTrackId.HasValue)
            {
                currentTrack = await _playlistTrackRepository.GetByIdAsync(player.CurrentPlaylistTrackId.Value);
            }

            // 1. cleanup if requested
            if (currentTrack?.IsRequestedTrack == true)
            {
                await _playlistTrackRepository.RemoveByIdAsync(currentTrack.Id);
            }

            // 2. update player state
            player.CurrentPlaylistTrackId = newTrack.Id;
            player.PlaylistId = newTrack.PlaylistId;
            player.LastUpdatedAt = DateTime.UtcNow;

            await _musicPlayerRepository.UpdateAsync(player);
            await _musicPlayerRepository.SaveChangesAsync();

            return true;
        }
    }
}
