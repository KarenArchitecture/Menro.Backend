using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Menro.Application.Features.Icons.Interfaces;
using Menro.Application.Features.Icons.DTOs;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Icons.Services
{
    public class IconService : IIconService
    {
        #region DI
        private readonly IIconRepository _repo;
        private readonly IMediaStorageProvider _mediaStorage;

        public IconService(IIconRepository repo, IMediaStorageProvider mediaStorage)
        {
            _repo = repo;
            _mediaStorage = mediaStorage;
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
                Url = _mediaStorage.GetUrl(MediaCategory.FoodCategoryIcon, x.FileName)
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
        public async Task<bool> AddAsync(string label, IFormFile icon)
        {
            if (icon == null || icon.Length == 0)
                throw new ArgumentException("فایلی برای آپلود ارسال نشده است.");

            if (!icon.FileName.ToLower().EndsWith(".svg"))
                throw new ArgumentException("فقط فایل‌های svg مجاز هستند.");

            var desiredFileName = Path.GetFileName(icon.FileName);

            // چک تکراری بودن قبل از آپلود، نه بعدش (جلوگیری از فایل orphan)
            var existingIcons = await _repo.GetAllAsync();
            if (existingIcons.Any(i => i.FileName.Equals(desiredFileName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("آیکونی با همین نام از قبل وجود دارد.");

            var result = await _mediaStorage.SaveAsync(MediaCategory.FoodCategoryIcon, icon);

            var entity = new Icon
            {
                FileName = result.FileName,
                Label = label?.Trim() ?? ""
            };

            var success = await _repo.AddAsync(entity);

            if (!success)
                _mediaStorage.Delete(MediaCategory.FoodCategoryIcon, result.FileName); // rollback

            return success;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var icon = await _repo.GetByIdAsync(id);
            if (icon == null)
                throw new InvalidOperationException("Icon not found");

            var dbResult = await _repo.DeleteAsync(id);
            if (!dbResult)
                return false;

            // delete icon file when deleting from db is successful
            _mediaStorage.Delete(MediaCategory.FoodCategoryIcon, icon.FileName);

            return true;
        }
    }
}
