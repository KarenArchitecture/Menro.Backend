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
        private readonly IFileUrlService _fileUrlService;

        public IconService(IIconRepository repo, IFileUrlService fileUrlService)
        {
            _repo = repo;
            _fileUrlService = fileUrlService;
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
        public async Task<bool> DeleteAsync(int id)
        {
            // ۱. آیکن رو از دیتابیس پیدا کن
            var icon = await _repo.GetByIdAsync(id);
            if (icon == null)
                throw new InvalidOperationException($"Icon with ID {id} not found.");

            // ۲. مسیر فیزیکی فایل واقعی آیکن (بر اساس منطق FileUrlService)
            // چون BuildIconUrl از "icons/{fileName}" استفاده می‌کند، مسیر فیزیکی هم باید همان باشد
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "icons", icon.FileName);

            // ۳. حذف فایل اگر وجود دارد
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"🗑 Icon file deleted successfully: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to delete icon file '{filePath}': {ex.Message}");
                    // حذف دیتابیس را ادامه بده چون ممکن است فایل قبلاً حذف شده باشد
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Icon file not found on disk: {filePath}");
            }

            // ۴. حذف رکورد از دیتابیس
            return await _repo.DeleteAsync(id);
        }
    }
}
