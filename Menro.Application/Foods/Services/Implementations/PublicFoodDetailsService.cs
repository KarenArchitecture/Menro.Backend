using Menro.Application.Common.Interfaces;
using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services.Implementations
{
    public class PublicFoodDetailsService : IPublicFoodDetailsService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IFileUrlService _fileUrlService;

        public PublicFoodDetailsService(
            IFoodRepository foodRepository,
            IFileUrlService fileUrlService)
        {
            _foodRepository = foodRepository;
            _fileUrlService = fileUrlService;
        }

        public async Task<PublicFoodDetailDto?> GetFoodDetailsAsync(int foodId)
        {
            var f = await _foodRepository.GetFoodWithVariantsAsync(foodId);
            if (f == null) return null;

            var dto = new PublicFoodDetailDto
            {
                Id = f.Id,
                Name = f.Name,
                Ingredients = f.Ingredients,
                BasePrice = f.Price,
                ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                    ? string.Empty
                    : _fileUrlService.BuildFoodImageUrl(f.ImageUrl),
                AverageRating = f.AverageRating,
                VotersCount = f.VotersCount
            };

            dto.Variants = f.Variants
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.Price)
                .Select(v => new PublicFoodVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    IsDefault = v.IsDefault ?? false,
                    Addons = v.Addons.Select(a => new PublicFoodAddonDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice
                    }).ToList()
                }).ToList();

            return dto;
        }
    }
}
