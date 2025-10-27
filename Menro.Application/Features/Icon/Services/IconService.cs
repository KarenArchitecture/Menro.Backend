using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;

namespace Menro.Application.Features.Icons.Services
{
    public class IconService : IIconService
    {
        private readonly IIconRepository _repo;
        private readonly IFileUrlService _fileUrlService;

        public IconService(IIconRepository repo, IFileUrlService fileUrlService)
        {
            _repo = repo;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<GetIconDto>> GetAllAsync()
        {
            var icons = await _repo.GetAllAsync();

            return icons.Select(x => new GetIconDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Label = x.Label,
                Url = _fileUrlService.BuildIconUrl(x.FileName)
            }).ToList();
        }

        public async Task<GetIconDto?> GetByIdAsync(int id)
        {
            var icon = await _repo.GetByIdAsync(id);
            if (icon == null) return null;

            return new GetIconDto
            {
                Id = icon.Id,
                FileName = icon.FileName,
                Label = icon.Label,
                Url = _fileUrlService.BuildIconUrl(icon.FileName)
            };
        }

        public async Task<bool> AddAsync(AddIconDto dto)
        {
            // ✅ اعتبارسنجی ساده
            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new ArgumentException("File name is required.");

            // بررسی تکراری بودن فایل
            var existingIcons = await _repo.GetAllAsync();
            if (existingIcons.Any(i => i.FileName.ToLower() == dto.FileName.ToLower()))
                throw new InvalidOperationException("An icon with the same file name already exists.");

            // ✅ ساخت entity
            var entity = new Icon
            {
                FileName = dto.FileName,
                Label = dto.Label
            };

            // ✅ ذخیره در دیتابیس
            return await _repo.AddAsync(entity);
            
        }
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
