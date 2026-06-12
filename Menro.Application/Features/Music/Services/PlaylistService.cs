using Menro.Application.Features.Music.DTOs;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.Music.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IUnitOfWork _uow;

        public PlaylistService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<PlaylistItemDto>> GetAllAsync(int restaurantId)
        {
            var playlists = await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId);

            var dto  = playlists.Select(x => new PlaylistItemDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                Tracks = x.Tracks.Count
            })
            .ToList();
            return dto;
        }

        public async Task<PlaylistDto?> GetByIdAsync(Guid playlistId, int restaurantId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null || playlist.RestaurantId != restaurantId)
                return null;

            var list = new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                IsActive = playlist.IsActive,

                Tracks = playlist.Tracks
                    .OrderBy(t => t.SortOrder)
                    .Select(t => new PlaylistTrackDto
                    {
                        Id = t.Id,
                        MusicTrackId = t.MusicTrackId,

                        Title = t.MusicTrack.Title,
                        Artist = t.MusicTrack.Artist,

                        CoverUrl = t.MusicTrack.CoverFileName,
                        AudioUrl = t.MusicTrack.AudioFileName,

                        Duration = t.MusicTrack.Duration,
                        SortOrder = t.SortOrder
                    })
                    .ToList()
            };


            return list;
        }

        // add track to playlist
        public async Task<bool> AddTrackAsync(Guid playlistId, int restaurantId, Guid musicTrackId)
        {
            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null)
                return false;

            if (playlist.RestaurantId != restaurantId)
                return false;

            var lastOrder = playlist.Tracks.Count == 0
                ? 0
                : playlist.Tracks.Max(x => x.SortOrder);

            var entity = new PlaylistTrack
            {
                PlaylistId = playlistId,
                MusicTrackId = musicTrackId,
                SortOrder = lastOrder + 1
            };

            await _uow.PlaylistTrack.AddAsync(entity);

            await _uow.SaveChangesAsync();

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

            _uow.PlaylistTrack.Remove(entity);

            await _uow.SaveChangesAsync();

            return true;
        }

    }
}
