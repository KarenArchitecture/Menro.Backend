// Application/Features/FoodCombos/Services/Implementations/FoodComboService.cs
using Menro.Application.Common.Interfaces;
using Menro.Application.Features.FoodCombos.DTOs;
using Menro.Application.Features.FoodCombos.Services.Interfaces;
using Menro.Domain.Interfaces;

namespace Menro.Application.Features.FoodCombos.Services.Implementations
{
    public class FoodComboService : IFoodComboService
    {
        private readonly IFoodComboRepository _comboRepository;
        private readonly IFileUrlService _fileUrlService;

        public FoodComboService(IFoodComboRepository comboRepository, IFileUrlService fileUrlService)
        {
            _comboRepository = comboRepository;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<int>> GetComboFoodIdsAsync(int foodId, int restaurantId)
        {
            var ownerRestaurantId = await _comboRepository.GetRestaurantIdForFoodAsync(foodId);
            if (ownerRestaurantId != restaurantId)
                return new List<int>();

            return await _comboRepository.GetComboFoodIdsAsync(foodId);
        }

        public async Task<(bool Success, string? Error)> SetCombosAsync(int foodId, List<int> comboFoodIds, int restaurantId)
        {
            var ownerRestaurantId = await _comboRepository.GetRestaurantIdForFoodAsync(foodId);
            if (ownerRestaurantId == null)
                return (false, "غذا یافت نشد.");

            if (ownerRestaurantId != restaurantId)
                return (false, "شما اجازه ویرایش ترکیب‌های این غذا را ندارید.");

            // every candidate combo food must also belong to this same restaurant
            foreach (var comboId in comboFoodIds.Distinct())
            {
                if (comboId == foodId) continue;

                var comboOwnerId = await _comboRepository.GetRestaurantIdForFoodAsync(comboId);
                if (comboOwnerId != restaurantId)
                    return (false, "همه غذاهای ترکیب باید متعلق به همین رستوران باشند.");
            }

            await _comboRepository.ReplaceCombosAsync(foodId, comboFoodIds);
            return (true, null);
        }

        public async Task<List<PublicComboFoodDto>> GetPublicCombosAsync(int foodId)
        {
            var foods = await _comboRepository.GetComboFoodsAsync(foodId);

            return foods.Select(f => new PublicComboFoodDto
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                    ? null
                    : _fileUrlService.BuildFoodImageUrl(f.ImageUrl),
                Price = f.Price,
                Rating = f.AverageRating,
                VotersCount = f.VotersCount,
                Variants = f.Variants.Select(v => new PublicComboVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    IsDefault = v.IsDefault ?? false,
                    Addons = v.Addons.Select(a => new PublicComboAddonDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ExtraPrice = a.ExtraPrice
                    }).ToList()
                }).ToList()
            }).ToList();
        }

        public async Task<Dictionary<int, int>> GetComboCountsAsync(int restaurantId)
        {
            return await _comboRepository.GetComboCountsByRestaurantAsync(restaurantId);
        }
    }
}