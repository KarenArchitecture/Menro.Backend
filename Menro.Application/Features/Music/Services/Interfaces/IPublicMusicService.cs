using Menro.Application.Common.Models;
using Menro.Application.Features.Music.DTOs.Public;

namespace Menro.Application.Features.Music.Services.Interfaces
{
    public interface IPublicMusicService
    {
        Task<PublicMusicPageDto?> GetPageAsync(int restaurantId, string userId);

        Task<Result> RequestTrackAsync(int restaurantId, string userId, Guid playlistTrackId);
    }
}
