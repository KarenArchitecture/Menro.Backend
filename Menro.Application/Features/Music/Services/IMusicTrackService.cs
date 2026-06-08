using Menro.Application.Features.Music.DTOs;
using Menro.Domain.Entities.Music;

namespace Menro.Application.Features.Music.Services
{
    public interface IMusicTrackService
    {
        Task<MusicTrack> CreateAsync(int restaurantId, CreateMusicTrackDto dto);
        Task<List<MusicTrackListItemDto>> GetAllAsync(int restaurantId);
        Task<MusicTrackDto?> GetByIdAsync(Guid trackId, int restaurantId);
        Task<MusicTrack?> RemoveAsync(Guid trackId, int restaurantId);
        Task<bool> UpdateAsync(Guid trackId,int restaurantId,UpdateMusicTrackDto dto);
    }
}
