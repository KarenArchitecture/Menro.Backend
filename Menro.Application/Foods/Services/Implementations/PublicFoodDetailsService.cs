using Menro.Application.Foods.DTOs;
using Menro.Application.Foods.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Foods.Services.Implementations
{
    public class PublicFoodDetailsService : IPublicFoodDetailsService
    {
        private readonly IFoodRepository _foodRepository;

        public PublicFoodDetailsService(IFoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }

        public async Task<PublicFoodDetailDto?> GetFoodDetailsAsync(int foodId)
        {
            var f = await _foodRepository.GetFoodWithVariantsAsync(foodId);
            if (f == null) return null;

            // Map to DTO
            var dto = new PublicFoodDetailDto
            {
                Id = f.Id,
                Name = f.Name,
                Ingredients = f.Ingredients,
                BasePrice = f.Price,
                ImageUrl = f.ImageUrl,
                AverageRating = f.AverageRating,
                VotersCount = f.VotersCount
            };

            // Map variants
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
