using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;

namespace Menro.Application.Features.Icons.Services
{
    public class IconService : IIconService
    {
        #region DI
        private readonly IIconRepository _repo;

        public IconService(IIconRepository repo)
        {
            _repo = repo;
        }
        #endregion
        public async Task<List<GetIconDto>> GetAllAsync()
        {
            var icons = await _repo.GetAllAsync();

            return icons.Select(x => new GetIconDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Label = x.Label,
                Url = x.FileName
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
                Url = icon.FileName
            };
        }
        public async Task<bool> AddAsync(string label, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.");

            // duplicate file?
            var existingIcons = await _repo.GetAllAsync();
            if (existingIcons.Any(i => i.FileName.ToLower() == fileName.ToLower()))
                throw new InvalidOperationException("An icon with the same file name already exists.");
            
            label = label?.Trim() ?? "";

            var entity = new Icon
            {
                FileName = fileName,
                Label = label
            };

            return await _repo.AddAsync(entity);
            
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var icon = await _repo.GetByIdAsync(id);
            if (icon == null)
                throw new InvalidOperationException("Icon not found");

            return await _repo.DeleteAsync(id);
        }
    }
}
