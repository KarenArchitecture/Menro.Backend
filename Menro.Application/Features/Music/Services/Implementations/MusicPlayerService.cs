using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces.Music;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class MusicPlayerService : IMusicPlayerService
    {
        private readonly IMusicPlayerRepository _musicPlayerRepository;
        private readonly IPlaylistTrackRepository _playlistTrackRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IMusicNotificationService _notification;

        public MusicPlayerService(IMusicPlayerRepository musicPlayerRepository, IPlaylistTrackRepository playlistTrackRepository, IPlaylistRepository playlistRepository, IMusicNotificationService notification)
        {
            _musicPlayerRepository = musicPlayerRepository;
            _playlistTrackRepository = playlistTrackRepository;
            _playlistRepository = playlistRepository;
            _notification = notification;
        }

        public async Task<MusicPlayerDto?> GetPlayerAsync(int restaurantId)
        {
            var player = await GetOrCreatePlayerAsync(restaurantId);

            return new MusicPlayerDto
            {
                PlaylistId = player.PlaylistId,
                CurrentTrackId = player.CurrentPlaylistTrackId
            };
        }

        public async Task<bool> SetCurrentTrackAsync(int restaurantId, Guid playlistId, Guid playlistTrackId)
        {
            var track = await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        public async Task<bool> AdvanceTrackAsync(int restaurantId, Guid playlistTrackId)
        {
            var track = await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        public async Task<bool> MoveToPreviousAsync(int restaurantId, Guid playlistTrackId)
        {
            var track = await _playlistTrackRepository.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            return await ChangeTrackAsync(restaurantId, track);
        }

        public async Task<MusicPlayer> GetOrCreatePlayerAsync(int restaurantId)
        {
            var player = await _musicPlayerRepository.GetByRestaurantIdAsync(restaurantId);

            if (player == null)
            {
                var playlist = await _playlistRepository.GetActiveByRestaurantIdAsync(restaurantId);

                if (playlist == null)
                    throw new Exception("No active playlist found for restaurant");

                var firstTrack = await _playlistTrackRepository.GetFirstByPlaylistIdAsync(playlist.Id);

                player = new MusicPlayer
                {
                    RestaurantId = restaurantId,
                    PlaylistId = playlist.Id,
                    CurrentPlaylistTrackId = firstTrack?.Id,
                    LastUpdatedAt = DateTime.UtcNow
                };

                await _musicPlayerRepository.CreateAsync(player);

                await BroadcastPlaybackChanged(player);

                return player;
            }

            await NormalizePlayerState(player);

            return player;
        }

        private async Task NormalizePlayerState(MusicPlayer player)
        {
            bool changed = false;

            if (!player.PlaylistId.HasValue)
                return;

            var playlist = await _playlistRepository.GetByIdAsync(player.PlaylistId.Value);

            if (playlist == null)
                return;

            if (!player.CurrentPlaylistTrackId.HasValue)
            {
                var firstTrack = await _playlistTrackRepository.GetFirstByPlaylistIdAsync(playlist.Id);

                if (firstTrack != null)
                {
                    player.CurrentPlaylistTrackId = firstTrack.Id;
                    changed = true;
                }
            }

            if (changed)
            {
                player.LastUpdatedAt = DateTime.UtcNow;

                await _musicPlayerRepository.UpdateAsync(player);

                await _musicPlayerRepository.SaveChangesAsync();

                await BroadcastPlaybackChanged(player);
            }
        }

        private async Task<bool> ChangeTrackAsync(int restaurantId, PlaylistTrack newTrack)
        {
            var player = await GetOrCreatePlayerAsync(restaurantId);

            PlaylistTrack? currentTrack = null;

            if (player.CurrentPlaylistTrackId.HasValue)
            {
                currentTrack = await _playlistTrackRepository.GetByIdAsync(player.CurrentPlaylistTrackId.Value);
            }

            if (currentTrack?.IsRequestedTrack == true)
            {
                await _playlistTrackRepository.RemoveByIdAsync(currentTrack.Id);
            }

            player.CurrentPlaylistTrackId = newTrack.Id;
            player.PlaylistId = newTrack.PlaylistId;
            player.LastUpdatedAt = DateTime.UtcNow;

            await _musicPlayerRepository.UpdateAsync(player);
            await _musicPlayerRepository.SaveChangesAsync();

            await BroadcastPlaybackChanged(player);

            return true;
        }

        // private helper for notifier
        private async Task BroadcastPlaybackChanged(MusicPlayer player)
        {
            await _notification.NotifyPlaybackChanged(
                player.RestaurantId,
                new MusicPlayerDto
                {
                    PlaylistId = player.PlaylistId,
                    CurrentTrackId = player.CurrentPlaylistTrackId
                });
        }
    }
}