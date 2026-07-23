using Menro.Application.Features.Music.DTOs.Archive;
using Menro.Domain.Entities.Music;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.Services.Interfaces
{
    public interface IMusicTrackService
    {
        Task<MusicTrackListItemDto> CreateAsync(int restaurantId, IFormFile audioFile, IFormFile? coverFile);
        Task<List<MusicTrackListItemDto>> GetAllAsync(int restaurantId);
        Task<MusicTrackDto?> GetByIdAsync(Guid trackId, int restaurantId);
        Task<string?> GetAudioPhysicalPathAsync(Guid trackId, int restaurantId);
        Task<MusicTrack?> RemoveAsync(Guid trackId, int restaurantId);
        Task<bool> UpdateAsync(Guid trackId, int restaurantId, UpdateMusicTrackDto dto);
    }
}
