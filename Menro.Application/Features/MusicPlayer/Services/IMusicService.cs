using Menro.Application.Features.MusicPlayer.Dtos;

namespace Menro.Application.Features.MusicPlayer.Services
{
    public interface IMusicService
    {
        Task CreateAsync(CreateMusicDto dto, string musicFilePath, string? coverFilePath);
        Task UpdateAsync(UpdateMusicDto dto);
        Task DeleteAsync(Guid id);

        Task<MusicDetailsDto?> GetByIdAsync(Guid id);
        Task<List<MusicListItemDto>> GetListAsync(string? searchTerm);
    }
}
