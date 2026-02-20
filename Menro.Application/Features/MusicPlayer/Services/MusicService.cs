using Menro.Application.Features.MusicPlayer.Dtos;
using Menro.Domain.Interfaces;
using Menro.Domain.Entities;

namespace Menro.Application.Features.MusicPlayer.Services
{
    public class MusicService : IMusicService
    {
        private readonly IMusicRepository _musicRepository;

        public MusicService(IMusicRepository musicRepository)
        {
            _musicRepository = musicRepository;
        }

        public async Task CreateAsync(
            CreateMusicDto dto,
            string musicFilePath,
            string? coverFilePath)
        {
            // Duration بعداً از metadata استخراج می‌شه
            var music = new Music(
                dto.Title,
                dto.Artist,
                TimeSpan.Zero,
                musicFilePath,
                coverFilePath
            );

            await _musicRepository.AddAsync(music);
        }

        public async Task UpdateAsync(UpdateMusicDto dto)
        {
            var music = await _musicRepository.GetByIdAsync(dto.Id);
            if (music == null)
                throw new Exception("Music not found");

            music.UpdateInfo(dto.Title, dto.Artist);

            if (dto.IsActive)
                music.Activate();
            else
                music.Deactivate();

            await _musicRepository.UpdateAsync(music);
        }

        public async Task DeleteAsync(Guid id)
        {
            var music = await _musicRepository.GetByIdAsync(id);
            if (music == null)
                throw new Exception("Music not found");

            await _musicRepository.DeleteAsync(music);
        }

        public async Task<MusicDetailsDto?> GetByIdAsync(Guid id)
        {
            var music = await _musicRepository.GetByIdAsync(id);
            if (music == null)
                return null;

            return new MusicDetailsDto
            {
                Id = music.Id,
                Title = music.Title,
                Artist = music.Artist,
                Duration = music.Duration,
                FileUrl = music.FilePath,
                CoverUrl = music.CoverPath,
                IsActive = music.IsActive
            };
        }

        public async Task<List<MusicListItemDto>> GetListAsync(string? searchTerm)
        {
            var musics = string.IsNullOrWhiteSpace(searchTerm)
                ? await _musicRepository.GetAllAsync()
                : await _musicRepository.SearchAsync(searchTerm);

            return musics.Select(m => new MusicListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Artist = m.Artist,
                Duration = m.Duration,
                IsActive = m.IsActive
            }).ToList();
        }
    }
}
