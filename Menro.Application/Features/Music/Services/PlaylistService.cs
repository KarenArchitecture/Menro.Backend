using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs;
using Menro.Domain.Entities.Music;
using Menro.Domain.Interfaces;
using Menro.Domain.Interfaces.Music;

namespace Menro.Application.Features.Music.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IUnitOfWork _uow;

        public PlaylistService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Playlist> CreateAsync(int restaurantId, CreatePlaylistDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Playlist name is required.");

            var exists = await _uow.Playlist.ExistsAsync(restaurantId, dto.Name);

            if (exists)
                throw new InvalidOperationException(
                    "Playlist already exists.");
            var hasPlaylist = (await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId)).Any();
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Name = dto.Name.Trim(),
                IsActive = !hasPlaylist,
            };

            await _uow.Playlist.AddAsync(playlist);
            await _uow.SaveChangesAsync();

            return playlist;
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

        public async Task<bool> RenameAsync(Guid playlistId, int restaurantId, RenamePlaylistDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Playlist name is required.");

            var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null)
                return false;

            var newName = dto.Name.Trim();

            var duplicateExists = await _uow.Playlist.ExistsAsync(restaurantId, newName);

            if (duplicateExists &&!string.Equals(playlist.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Playlist name already exists.");
            }

            playlist.Name = newName;

            var updated = await _uow.Playlist.UpdateAsync(playlist);

            if (!updated)
                return false;

            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<Result> DeletePlaylistAsync(int restaurantId, Guid playlistId)
        {
            //var playlist = await _uow.Playlist.GetByIdAsync(playlistId);

            //if (playlist is null)
            //    return Result.Failure("Playlist not found.");

            //if (playlist.RestaurantId != restaurantId)
            //    return Result.Failure("Access denied.");

            await _uow.Playlist.DeleteAsync(playlistId);
            await _uow.SaveChangesAsync();

            return Result.Success();
        }


        public async Task<bool> SetActiveAsync(Guid playlistId, int restaurantId)
        {
            var playlists = await _uow.Playlist.GetAllByRestaurantIdAsync(restaurantId);

            var selected = playlists.FirstOrDefault(x => x.Id == playlistId);

            if (selected == null)
                return false;

            foreach (var playlist in playlists)
            {
                playlist.IsActive = playlist.Id == playlistId;

                await _uow.Playlist.UpdateAsync(playlist);
            }

            await _uow.SaveChangesAsync();

            return true;
        }


        /*-----------------*/
        /* --- Tracks --- */
        /*---------------*/

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

        // rename track title
        public async Task<bool> RenameAsync(Guid trackId, int restaurantId, RenameMusicTrackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            var track = await _uow.MusicTrack.GetByIdAsync(trackId, restaurantId);

            if (track == null)
                return false;

            track.Title = dto.Title.Trim();

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

        // re-order track in playlist
        public async Task<bool> ReorderTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId, PlaylistTrackMoveDirection direction)
        {
            var playlist =
                await _uow.Playlist.GetByIdAsync(playlistId);

            if (playlist == null)
                return false;

            if (playlist.RestaurantId != restaurantId)
                return false;

            var track = await _uow.PlaylistTrack.GetByIdAsync(playlistTrackId);

            if (track == null)
                return false;

            PlaylistTrack? swapTrack = null;

            if (direction.ToString().ToLower() == "up")
            {
                swapTrack = await _uow.PlaylistTrack.GetPreviousTrackAsync(playlistId, track.SortOrder);
            }
            else if (direction.ToString().ToLower() == "down")
            {
                swapTrack = await _uow.PlaylistTrack.GetNextTrackAsync(playlistId, track.SortOrder);
            }
            else
            {
                throw new ArgumentException(
                    "Invalid direction.");
            }

            if (swapTrack == null)
                return true;

            var currentOrder = track.SortOrder;

            track.SortOrder = swapTrack.SortOrder;
            swapTrack.SortOrder = currentOrder;

            await _uow.SaveChangesAsync();

            return true;
        }
    }
}
