using Menro.Application.Features.Music.DTOs.Player;
using Menro.Domain.Entities.Music;

namespace Menro.Domain.Interfaces.Music
{
    public interface IMusicPlayerService
    {
        Task<MusicPlayerDto?> GetPlayerAsync(int restaurantId);
        Task<bool> SetCurrentTrackAsync(int restaurantId, Guid playlistId, Guid playlistTrackId);
        Task<bool> AdvanceTrackAsync(int restaurantId, Guid playlistTrackId);
        Task<bool> MoveToPreviousAsync(int restaurantId, Guid playlistTrackId);
        Task<MusicPlayer> GetOrCreatePlayerAsync(int restaurantId);
        //Task<MusicPlayer> EnsureMusicPlayerExistsAsync(int restaurantId);

    }
}
