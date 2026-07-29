using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services.Implementations
{
    public class PublicFoodDetailsService : IPublicFoodDetailsService
    {
        private readonly IFoodRepository _foodRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public PublicFoodDetailsService(
            IFoodRepository foodRepository,
            IMediaStorageProvider mediaStorage)
        {
            _foodRepository = foodRepository;
            _mediaStorage = mediaStorage;
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
                    : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, entityId: f.Id.ToString(), variant: MediaVariant.Resized),
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
