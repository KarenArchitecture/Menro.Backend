using Menro.Application.Features.Music.DTOs;

namespace Menro.Application.Features.Music.Services
{
    public interface IPlaylistService
    {
        Task<List<PlaylistItemDto>> GetAllAsync(int restaurantId);
        Task<PlaylistDto?> GetByIdAsync(Guid playlistId, int restaurantId);

        Task<bool> AddTrackAsync(Guid playlistId, int restaurantId, Guid musicTrackId);
        Task<bool> RemoveTrackAsync(Guid playlistId, int restaurantId, Guid playlistTrackId);

    }
}
