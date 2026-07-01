using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs.Player;
using Menro.Application.Features.Music.DTOs.Playlist;
using Menro.Application.Features.Music.Services.Interfaces;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Music.Services.Implementations
{
    public class PlaylistService : IPlaylistService
    {
        #region DI
        private readonly IUnitOfWork _uow;
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly IMusicNotificationService _notification;

        public PlaylistService(IUnitOfWork uow, IMusicPlayerService musicPlayerService, IMusicNotificationService notification)
        {
            _uow = uow;
            _musicPlayerService = musicPlayerService;
            _notification = notification;
        }
        #endregion

        // create playlist
        public async Task<Playlist> CreateAsync(int restaurantId, CreatePlaylistDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Playlist name is required.");

        var exists = await _uow.Playlist.ExistsAsync(restaurantId, dto.Name);

        if (exists)
            throw new InvalidOperationException("Playlist already exists.");

        var playlist = new Playlist
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = dto.Name.Trim(),
            IsActive = false
        };

        await _uow.Playlist.AddAsync(playlist);
        await _uow.SaveChangesAsync();

        await SetActivePlaylistAsync(playlist.Id, restaurantId);

            await _notification.NotifyPlaylistChanged(restaurantId);

            return playlist;
        }

        // get all playlists
        public async Task<List<PlaylistItemDto>> GetAllAsync(int restaurantId)
        {
            var playlists = await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId);

            return playlists.Select(x => new PlaylistItemDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                Tracks = x.Tracks.Count
            }).ToList();
        }

        // get playlist with tracks
        public async Task<PlaylistDto?> GetByIdAsync(Guid playlistId, int restaurantId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

        if (playlist == null || playlist.RestaurantId != restaurantId)
            return null;

            return new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                IsActive = playlist.IsActive,
                Tracks = playlist.Tracks.OrderBy(t => t.SortOrder).Select(t => new PlaylistTrackDto
                {
                    Id = t.Id,
                    MusicTrackId = t.MusicTrackId,
                    Title = t.MusicTrack.Title,
                    Artist = t.MusicTrack.Artist,
                    CoverUrl = t.MusicTrack.CoverFileName,
                    AudioUrl = t.MusicTrack.AudioFileName,
                    Duration = t.MusicTrack.Duration,
                    SortOrder = t.SortOrder,
                    IsRequestedTrack = t.IsRequestedTrack
                }).ToList()
            };
        }

        // rename playlist
        public async Task<bool> RenameAsync(Guid playlistId, int restaurantId, RenamePlaylistDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Playlist name is required.");

        var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

        if (playlist == null)
            return false;

        var newName = dto.Name.Trim();

        var duplicateExists = await _uow.Playlist.ExistsAsync(restaurantId, newName);

            if (duplicateExists && !string.Equals(playlist.Name, newName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Playlist name already exists.");

        playlist.Name = newName;

            await _uow.Playlist.UpdateAsync(playlist);
            await _uow.SaveChangesAsync();

            await _notification.NotifyPlaylistChanged(restaurantId);

            return true;
        }

        // remove playlist from restaurant + all it's tracks
        public async Task<Result> DeletePlaylistAsync(int restaurantId, Guid playlistId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null || playlist.RestaurantId != restaurantId)
                return Result.Failure("Playlist not found.");

        if (playlist.IsActive)
            return Result.Failure("پلی‌لیست فعال قابل حذف نیست.");

        await _uow.Playlist.DeleteAsync(playlist);
        await _uow.SaveChangesAsync();

            await _notification.NotifyPlaylistChanged(restaurantId);

            return Result.Success();
        }

        // activate playlist
        public async Task<bool> SetActivePlaylistAsync(Guid playlistId, int restaurantId)
    {
        var playlists = await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId);

            var selected = playlists.FirstOrDefault(x => x.Id == playlistId);

            if (selected == null)
                return false;

        foreach (var playlist in playlists)
        {
            playlist.IsActive = playlist.Id == playlistId;
        }

        var player = await _musicPlayerService.GetOrCreatePlayerAsync(restaurantId);

            var firstTrack = selected.Tracks.OrderBy(x => x.SortOrder).FirstOrDefault();

        player.PlaylistId = selected.Id;
        player.CurrentPlaylistTrackId = firstTrack?.Id;
        player.LastUpdatedAt = DateTime.UtcNow;

            await _uow.MusicPlayer.UpdateAsync(player);
            await _uow.SaveChangesAsync();

            await _notification.NotifyPlaylistChanged(restaurantId);

            return true;
        }

        /*---------------------*/
        /*--------TRACKS-------*/
        /*---------------------*/

        // add track from archive to playlist
        public async Task<bool> AddTrackAsync(Guid playlistId, int restaurantId, Guid musicTrackId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null || playlist.RestaurantId != restaurantId)
                return false;

            var lastOrder = playlist.Tracks.Count == 0 ? 0 : playlist.Tracks.Max(x => x.SortOrder);

            var entity = new PlaylistTrack
            {
                PlaylistId = playlistId,
                MusicTrackId = musicTrackId,
                SortOrder = lastOrder + 1
            };

            await _uow.PlaylistTrack.AddAsync(entity);
            await _uow.SaveChangesAsync();

            await _notification.NotifyPlaylistChanged(restaurantId);

            return true;
        }

        // remove track from playlist
        public async Task<bool> RemoveTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null || playlist.RestaurantId != restaurantId)
                return false;

            var entity = await _uow.PlaylistTrack.GetByIdAsync(playlistTrackId);

            if (entity == null)
                return false;

            var player = await _uow.MusicPlayer.GetByRestaurantIdAsync(restaurantId);

            bool playbackChanged = false;

            if (player?.CurrentPlaylistTrackId == playlistTrackId)
            {
                var currentTrack = await _uow.PlaylistTrack.GetByIdAsync(playlistTrackId);

                if (currentTrack == null)
                    return false;

                var nextTrack = await _uow.PlaylistTrack.GetNextTrackAsync(
                    currentTrack.PlaylistId,
                    currentTrack.SortOrder
                );

                if (nextTrack != null)
                {
                    player.CurrentPlaylistTrackId = nextTrack.Id;
                    player.PlaylistId = nextTrack.PlaylistId;
                }
                else
                {
                    player.CurrentPlaylistTrackId = null;
                }

                player.LastUpdatedAt = DateTime.UtcNow;

                await _uow.MusicPlayer.UpdateAsync(player);

                playbackChanged = true;
            }

            await _uow.PlaylistTrack.RemoveAsync(entity);
            await _uow.SaveChangesAsync();

            if (playbackChanged)
            {
                await _notification.NotifyPlaybackChanged(
                    restaurantId,
                    new MusicPlayerDto
                    {
                        PlaylistId = player?.PlaylistId,
                        CurrentTrackId = player?.CurrentPlaylistTrackId
                    });
            }

            await _notification.NotifyPlaylistChanged(restaurantId);

            return true;
        }

        // re-order playlistTracks
        public async Task<bool> ReorderTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId, PlaylistTrackMoveDirection direction)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null || playlist.RestaurantId != restaurantId)
                return false;

        var track = await _uow.PlaylistTrack.GetByIdAsync(playlistTrackId);

        if (track == null)
            return false;

        PlaylistTrack? swapTrack = null;

            if (direction.ToString().ToLower() == "up")
                swapTrack = await _uow.PlaylistTrack.GetPreviousTrackAsync(playlistId, track.SortOrder);

            else if (direction.ToString().ToLower() == "down")
                swapTrack = await _uow.PlaylistTrack.GetNextTrackAsync(playlistId, track.SortOrder);

            else
                throw new ArgumentException("Invalid direction.");

        if (swapTrack == null)
            return true;

        var currentOrder = track.SortOrder;

        track.SortOrder = swapTrack.SortOrder;
        swapTrack.SortOrder = currentOrder;

        await _uow.SaveChangesAsync();

            await _notification.NotifyPlaylistChanged(restaurantId);

            return true;
        }
    }
}

