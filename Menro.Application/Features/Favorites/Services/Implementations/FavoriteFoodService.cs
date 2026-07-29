using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Favorites.DTOs;
using Menro.Application.Features.Favorites.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Favorites.Services.Implementations
{
    public class FavoriteFoodService : IFavoriteFoodService
    {
        private readonly IFavoriteFoodRepository _repo;
        private readonly IFoodRepository _foodRepo;
        private readonly IMediaStorageProvider _mediaStorage;

        public FavoriteFoodService(
            IFavoriteFoodRepository repo,
            IFoodRepository foodRepo,
            IMediaStorageProvider mediaStorage)
        {
            _repo = repo;
            _foodRepo = foodRepo;
            _mediaStorage = mediaStorage;
        }

        public async Task ToggleAsync(string userId, int foodId)
        {
            var exists = await _repo.ExistsAsync(userId, foodId);
            if (exists)
            {
                var entity = await _repo.GetAsync(userId, foodId);
                if (entity != null)
                    await _repo.RemoveAsync(entity);
            }
            else
            {
                await _repo.AddAsync(new FavoriteFood
                {
                    UserId = userId,
                    FoodId = foodId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public async Task<List<FavoriteFoodDto>> GetUserFavoritesAsync(string userId)
        {
            var foodIds = await _repo.GetFavoriteFoodIdsByUserAsync(userId);
            if (!foodIds.Any())
                return new List<FavoriteFoodDto>();

            var foods = await _foodRepo.GetFoodsByIdsAsync(foodIds);
            return foods
                .Select(f => new FavoriteFoodDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ImageUrl = string.IsNullOrWhiteSpace(f.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, f.ImageUrl, f.Id.ToString(), MediaVariant.Thumbnail),
                    Price = f.Price,
                    Rating = f.AverageRating,
                    Voters = f.VotersCount,
                    RestaurantName = f.Restaurant.Name,
                    RestaurantId = f.RestaurantId,
                    RestaurantSlug = f.Restaurant.Slug
                })
                .ToList();
        }

        public async Task<List<int>> GetFavoriteFoodIdsAsync(string userId)
        {
            return await _repo.GetFavoriteFoodIdsByUserAsync(userId);
        }
    }
}