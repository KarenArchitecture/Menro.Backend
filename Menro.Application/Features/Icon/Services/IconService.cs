using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Microsoft.Extensions.Configuration;

namespace Menro.Application.Features.Icons.Services
{
    public class IconService : IIconService
    {
        private readonly IIconRepository _repo;
        private readonly IConfiguration _config;

        public IconService(IIconRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<List<GetIconDto>> GetAllAsync()
        {
            var icons = await _repo.GetAllAsync();
            var baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";

            return icons.Select(x => new GetIconDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Label = x.Label,
                Url = $"{baseUrl}/icons/{x.FileName}"
            }).ToList();
        }

        public async Task<GetIconDto?> GetByIdAsync(int id)
        {
            var icon = await _repo.GetByIdAsync(id);
            if (icon == null) return null;

            var baseUrl = _config["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";

            return new GetIconDto
            {
                Id = icon.Id,
                FileName = icon.FileName,
                Label = icon.Label,
                Url = $"{baseUrl}/icons/{icon.FileName}"
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
