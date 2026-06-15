using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs;
using Menro.Domain.Entities.Music;

namespace Menro.Application.Features.Music.Services
{
    public interface IPlaylistService
    {
        Task<Playlist> CreateAsync(int restaurantId, CreatePlaylistDto dto);
        Task<List<PlaylistItemDto>> GetAllAsync(int restaurantId);
        Task<PlaylistDto?> GetByIdAsync(Guid playlistId, int restaurantId);
        Task<bool> RenameAsync(Guid playlistId, int restaurantId, RenamePlaylistDto dto);
        Task<Result> DeletePlaylistAsync(int restaurantId, Guid playlistId);

        Task<bool> SetActiveAsync(Guid playlistId, int restaurantId);


        /*-----------------*/
        /* --- Tracks --- */
        /*---------------*/
        Task<bool> AddTrackAsync(Guid playlistId, int restaurantId, Guid musicTrackId);
        Task<bool> RenameAsync(Guid trackId, int restaurantId, RenameMusicTrackDto dto);
        Task<bool> RemoveTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId);
        Task<bool> ReorderTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId, PlaylistTrackMoveDirection direction);

    }
}
